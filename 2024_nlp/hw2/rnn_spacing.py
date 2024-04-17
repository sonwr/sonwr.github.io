import os
import numpy as np
import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import DataLoader, TensorDataset
from sklearn.metrics import accuracy_score
  
class SpacingRNN(nn.Module):
    def __init__(self, config):
        super(SpacingRNN, self).__init__()
        self.embedding = nn.Embedding(num_embeddings=config['eumjeol_vocab_size'], embedding_dim=config['embedding_size'], padding_idx=0)
        self.dropout = nn.Dropout(config['dropout'])
        # 다층 LSTM 적용
        self.lstm = nn.LSTM(input_size=config['embedding_size'], 
                            hidden_size=config['hidden_size'], 
                            num_layers=config['num_layers'],  # 층 수 추가
                            batch_first=True, 
                            bidirectional=True)
        self.linear = nn.Linear(in_features=config['hidden_size'] * 2, out_features=config['number_of_labels'])

    def forward(self, inputs):
        x = self.embedding(inputs)
        x = self.dropout(x)
        x, _ = self.lstm(x)
        x = self.dropout(x)
        x = self.linear(x)
        return x

def read_datas(file_path):
    with open(file_path, "r", encoding="utf8") as inFile:
        lines = inFile.readlines()
    datas = []
    for line in lines:
        pieces = line.strip().split("\t")
        eumjeol_sequence, label_sequence = pieces[0].split(), pieces[1].split()
        datas.append((eumjeol_sequence, label_sequence))
    return datas

def read_vocab_data(eumjeol_vocab_data_path):
    label2idx, idx2label = {"<PAD>":0, "B":1, "I":2}, {0:"<PAD>", 1:"B", 2:"I"}
    eumjeol2idx, idx2eumjeol = {}, {}
    with open(eumjeol_vocab_data_path, "r", encoding="utf8") as inFile:
        lines = inFile.readlines()
    for line in lines:
        eumjeol = line.strip()
        if eumjeol not in eumjeol2idx:
            eumjeol2idx[eumjeol] = len(eumjeol2idx)
            idx2eumjeol[eumjeol2idx[eumjeol]] = eumjeol
    return eumjeol2idx, idx2eumjeol, label2idx, idx2label

def load_dataset(config, data_path):
    datas = read_datas(data_path)
    eumjeol2idx, idx2eumjeol, label2idx, idx2label = read_vocab_data(config["eumjeol_vocab"])
    eumjeol_features, eumjeol_feature_lengths, label_features = [], [], []

    for eumjeol_sequence, label_sequence in datas:
        eumjeol_feature = [eumjeol2idx[eumjeol] for eumjeol in eumjeol_sequence]
        label_feature = [label2idx[label] for label in label_sequence]
        eumjeol_feature_length = len(eumjeol_feature)
        eumjeol_feature += [0] * (config["max_length"] - eumjeol_feature_length)
        label_feature += [0] * (config["max_length"] - eumjeol_feature_length)
        eumjeol_features.append(eumjeol_feature)
        eumjeol_feature_lengths.append(eumjeol_feature_length)
        label_features.append(label_feature)

    return torch.tensor(eumjeol_features, dtype=torch.long), torch.tensor(eumjeol_feature_lengths, dtype=torch.long), torch.tensor(label_features, dtype=torch.long), eumjeol2idx, idx2eumjeol, label2idx, idx2label

def train(config):
    device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
    model = SpacingRNN(config).to(device)
    eumjeol_features, eumjeol_feature_lengths, label_features, eumjeol2idx, idx2eumjeol, label2idx, idx2label = load_dataset(config, config["train_data"])
    train_features = TensorDataset(eumjeol_features, eumjeol_feature_lengths, label_features)
    train_dataloader = DataLoader(train_features, shuffle=True, batch_size=config["batch_size"])
    loss_func = nn.CrossEntropyLoss(ignore_index=0)
    optimizer = optim.Adam(model.parameters(), lr=0.001)

    for epoch in range(config["epoch"]):
        model.train()
        costs = []

        for step, batch in enumerate(train_dataloader):
            optimizer.zero_grad()
            batch = tuple(t.to(device) for t in batch)
            inputs, input_lengths, labels = batch[0], batch[1], batch[2]
            hypothesis = model(inputs)
            cost = loss_func(hypothesis.view(-1, config['number_of_labels']), labels.view(-1))
            cost.backward()
            optimizer.step()
            costs.append(cost.item())

        torch.save(model.state_dict(), os.path.join(config["output_dir"], f"epoch_{epoch + 1}.pt"))
        print(f"Epoch {epoch + 1}: Average Cost = {np.mean(costs)}")


def tensor2list(input_tensor):
    return input_tensor.cpu().detach().numpy().tolist()

def make_sentence(inputs, predicts, labels, idx2eumjeol, idx2label):
    predict_sentence, correct_sentence = "", ""
    for index in range(len(inputs)):
        eumjeol = idx2eumjeol[inputs[index]]
        correct_label = idx2label[labels[index]]
        predict_label = idx2label[predicts[index]]
        if (index == 0):
            predict_sentence += eumjeol
            correct_sentence += eumjeol
            continue
        if (predict_label == "B"):
            predict_sentence += " "
        predict_sentence += eumjeol
        if (correct_label == "B"):
            correct_sentence += " "
        correct_sentence += eumjeol
    return predict_sentence, correct_sentence

def test(config):
    device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
    eumjeol_features, eumjeol_feature_lengths, label_features, eumjeol2idx, idx2eumjeol, label2idx, idx2label = load_dataset(config, config["test_data"])
    test_features = TensorDataset(eumjeol_features, eumjeol_feature_lengths, label_features)
    test_dataloader = DataLoader(test_features, shuffle=False, batch_size=1)
    model = SpacingRNN(config).to(device)
    model.load_state_dict(torch.load(os.path.join(config["output_dir"], config["model_name"]), map_location=device))
    total_hypothesis, total_labels = [], []

    for step, batch in enumerate(test_dataloader):
        model.eval()
        batch = tuple(t.to(device) for t in batch)
        inputs, input_lengths, labels = batch[0], batch[1], batch[2]
        hypothesis = model(inputs)
        hypothesis = torch.argmax(hypothesis, dim=-1)
        input_length = tensor2list(input_lengths[0])
        input = tensor2list(inputs[0])[:input_length]
        label = tensor2list(labels[0])[:input_length]
        hypothesis = tensor2list(hypothesis[0])[:input_length]
        total_hypothesis += hypothesis
        total_labels += label
        if (step < 10): # 처음 10개만 화면에 예시로 출력
            predict_sentence, correct_sentence = make_sentence(input, hypothesis, label, idx2eumjeol, idx2label)
            print("정답 : " + correct_sentence)
            print("출력 : " + predict_sentence)
            print()
    print("Accuracy : {}".format(accuracy_score(total_labels, total_hypothesis)))
    

# Configuration for model and dataset
config = {
    "inter_layer_dropout": 0.3,  # LSTM 층 사이의 드롭아웃
    "num_layers": 5,  # LSTM 층 수
    "dropout": 0.3, # 드롭아웃 레이어: 과적합을 방지하기 위해 특정 비율(config["dropout"])로 뉴런의 출력을 임의로 0으로 설정
    "hidden_size": 64,  # RNN 히든 사이즈: LSTM 셀 또는 RNN 셀의 히든 상태의 차원 수를 설정. 네트워크의 메모리 용량을 결정
    "batch_size": 64,
    "epoch": 15,
    "number_of_labels": 3,  # 분류할 라벨의 개수: 모델 출력층에서 예측할 라벨(클래스)의 개수
    
    "eumjeol_vocab_size": 2000,  # 전체 음절 개수: 모델이 인식하고 처리할 수 있는 고유 음절의 총 수를 정의
    "embedding_size": 128,  # Embedding Size: 고차원 벡터를 훨씬 낮은 차원의 밀집 벡터(dense vector)로 변환
    "max_length": 920,  # Maximum length of sentences
    
    "train_data": "train.txt",  # Path to the training data
    "test_data": "test.txt",  # Path to the training data    
    "eumjeol_vocab": "eumjeol_vocab.txt",  # Path to the eumjeol vocabulary file
    "output_dir": "./model_output",  # Output directory for model checkpoints
    "model_name":"epoch_{0:d}.pt".format(5),
}

mode = input("Enter 'train' to train the model or 'test' to test: ").strip().lower()
if mode == 'train':
    train(config)
elif mode == 'test':
    test(config)
else:
    print("Invalid mode entered. Please enter 'train' or 'test'.")

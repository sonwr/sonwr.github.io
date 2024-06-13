import torch
from transformers import DistilBertTokenizerFast, DistilBertForSequenceClassification, Trainer, TrainingArguments
from datasets import load_dataset
import evaluate
import numpy as np

# 데이터셋 로드
dataset = load_dataset('imdb')

# 데이터셋을 학습 및 평가 세트로 분할
train_dataset = dataset['train'].shuffle(seed=42).select(range(2000))
test_dataset = dataset['test'].shuffle(seed=42).select(range(500))

# 토크나이저 및 모델 로드
tokenizer = DistilBertTokenizerFast.from_pretrained('distilbert-base-uncased')
model = DistilBertForSequenceClassification.from_pretrained('distilbert-base-uncased')

# 데이터셋을 토큰화
def tokenize_function(examples):
    return tokenizer(examples['text'], padding="max_length", truncation=True)

train_dataset = train_dataset.map(tokenize_function, batched=True)
test_dataset = test_dataset.map(tokenize_function, batched=True)

train_dataset = train_dataset.remove_columns(['text'])
test_dataset = test_dataset.remove_columns(['text'])

train_dataset.set_format('torch')
test_dataset.set_format('torch')

# 평가 메트릭 정의
metric = evaluate.load('accuracy')

def compute_metrics(eval_pred):
    logits, labels = eval_pred
    predictions = torch.argmax(torch.tensor(logits), dim=-1)
    return metric.compute(predictions=predictions, references=labels)

# 학습 인자 정의
training_args = TrainingArguments(
    output_dir='./results',
    eval_strategy="epoch",
    learning_rate=2e-5,
    per_device_train_batch_size=8,
    per_device_eval_batch_size=8,
    num_train_epochs=3,
    weight_decay=0.01,
)

# Trainer 정의
trainer = Trainer(
    model=model,
    args=training_args,
    train_dataset=train_dataset,
    eval_dataset=test_dataset,
    compute_metrics=compute_metrics,
)

# 모델 학습
trainer.train()

# 학습된 모델로 감정 분석 파이프라인 생성
from transformers import pipeline

sentiment_pipeline = pipeline("sentiment-analysis", model=model, tokenizer=tokenizer)

# 분석할 텍스트 정의
texts = [
    "I love this product! It is absolutely amazing.",
    "This is the worst experience I've ever had.",
    "The movie was okay, but not great.",
    "I am so happy with the service.",
    "I am disappointed with the quality of the item."
]

# 감정 분석 수행
results = sentiment_pipeline(texts)

# 결과 출력
for text, result in zip(texts, results):
    print(f"Text: {text}")
    print(f"Sentiment: {result}\n")

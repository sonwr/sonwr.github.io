# SpacingRNN

This project implements a SpacingRNN model for Korean word spacing using PyTorch.

## Introduction

The SpacingRNN model aims to automatically insert spaces between Korean words in a sentence. This task is crucial for natural language processing applications such as text segmentation, parsing, and machine translation. The model utilizes an enhanced bidirectional LSTM (Long Short-Term Memory) neural network architecture, upgraded to a multi-layer configuration to predict the optimal spacing between characters more effectively.

## Modifications and Enhancements

The original implementation from KUNLP has been significantly modified to include a multi-layer LSTM structure which allows the model to capture deeper linguistic structures and dependencies. The number of LSTM layers and the addition of inter-layer dropout are key hyperparameters that have been adjusted to optimize performance and combat overfitting.

### Key Enhancements Include:

- **Multi-Layer LSTM**: Increasing the number of LSTM layers helps the model to learn more complex patterns and dependencies.
- **Inter-Layer Dropout**: Added dropout between LSTM layers to prevent overfitting and improve generalization to new data.
- **Hyperparameter Tuning**: Adjustments in the learning rate, number of epochs, and batch size to better suit the multi-layer structure.

## Required Packages

To run this project, you need to have the following packages installed:

- `torch`: PyTorch deep learning framework.
- `numpy`: Numerical computing library for handling array operations.
- `scikit-learn`: Machine learning library for evaluating model performance.
- `os`: Operating system interfaces for accessing files and directories.

## Introduction to Data Files

### `train.txt`

The `train.txt` file is the training dataset used for training the SpacingRNN model. Each line consists of a Korean sentence without spaces and its corresponding spacing tags. The sentence and tags are separated by a tab.

### `test.txt`

The `test.txt` file is the test dataset used for evaluating the trained SpacingRNN model. It follows the same format as `train.txt`.

### `eumjeol_vocab.txt`

The `eumjeol_vocab.txt` file serves as the word vocabulary for Korean eumjeols (syllables). Each line represents a single eumjeol, with one eumjeol per line. This file is used when the model processes input data at the eumjeol level.

These files are utilized in training and evaluating the SpacingRNN model. The model is trained using the training data in `train.txt`, and its performance is evaluated using the test data in `test.txt`.

## Configuration Details

```yaml
eumjeol_vocab_size: 2000  # Number of unique syllables
embedding_size: 128       # Dimensionality of embedding space
hidden_size: 64           # Number of units in LSTM cells
dropout: 0.5              # Dropout rate for input/output layers
inter_layer_dropout: 0.3  # Dropout rate between LSTM layers
number_of_labels: 3       # Number of label classes (B, I, <PAD>)
num_layers: 3             # Number of LSTM layers
max_length: 920           # Maximum length of input sequences
batch_size: 64            # Number of sequences per batch
epoch: 10                 # Number of training iterations
```

## Running the SpacingRNN Model

### Running the Script

Navigate to the directory containing your script and run it using Python. You can specify whether to train or test the model using command-line input:

```
python rnn_spacing.py
```

After executing the script, you will be prompted to enter a mode (train or test). Input your choice based on what you want to do:
```
Enter 'train' to train the model or 'test' to test: train
```

Or for testing:
```
Enter 'train' to train the model or 'test' to test: test
```

### Console Output During Training:
Training the Model
```
python .\rnn_spacing.py
Enter 'train' to train the model or 'test' to test: train
Epoch 1: Average Cost = 0.6611276759377008
Epoch 2: Average Cost = 0.47395580912692636
Epoch 3: Average Cost = 0.2241853320900398
Epoch 4: Average Cost = 0.1873242076061949
Epoch 5: Average Cost = 0.16520120731637447
Epoch 6: Average Cost = 0.15290646170136296
Epoch 7: Average Cost = 0.13994104650956166
Epoch 8: Average Cost = 0.13051934351649466
Epoch 9: Average Cost = 0.124607874717139
Epoch 10: Average Cost = 0.11608582819941678
Epoch 11: Average Cost = 0.11178155337707905
Epoch 12: Average Cost = 0.10650203009194965
Epoch 13: Average Cost = 0.10131081448325628
Epoch 14: Average Cost = 0.09720204459338248
Epoch 15: Average Cost = 0.0936379850476603
```

Testing the Model
```
python .\rnn_spacing.py
Enter 'train' to train the model or 'test' to test: test
정답 : 그러고 보니 경리는 윤보혜의 근황에 대해 아는 것이 없어보였다.
출력 : 그러고 보니 경리는 윤보혜의 근황에 대해 아는 것이 없어 보였다.

정답 : 이제 7년 대환란이 눈앞에 닥쳐왔습니다.
출력 : 이제7년 대환란 이 눈 앞에 닥쳐 왔습니다.

정답 : 이 관찰자에게 장치(예를 들면 90의 경사각을 가진 두 거울)를 제공하여, 같은 시각에 A와 B 두 곳을 한꺼번에 관찰할 수 있게 한다.
출력 : 이관찰자에게 장치(예를 들면 90의 경사각을 가진 두 거울)를 제공하여, 같은 시각에 A와 B두 곳을 한 꺼번에 관찰할 수 있게 한다.

정답 : "먼저 약속을 어긴 쪽은 한달준 그 놈이었어."
출력 : "먼저 약속을 어긴 쪽은 한 달준 그 놈이었어."

정답 : 레이첼.
출력 : 레이첼.

정답 : 처남은 의사의 진단서를 북북 찢어버렸다.
출력 : 처남은 의사의 진단서를 북북찢어 버렸다.

정답 : 그 전화를 받았던 날, 과일을 벗기던 혜숙이가 물었었다.
출력 : 그 전화를 받았던 날, 과 일을 벗기던 혜숙이 가 물었었다.

정답 : "저도 처음에는 금변호사님처럼 그렇게 생각했지요. 그러나 범인은 경계선을 사이에 두고 한쪽 발은 테이블쪽에 두고 한쪽 발을 홀 중앙에 두고 있던 있던 게 분명해요."
출력 : "저도 처음에는 금변호사님처럼 그렇게 생각했지요. 그러나 범인은 경계선을 사이에 두고 한 쪽 발은 테이 블쪽에 두고 한 쪽 발을 홀 중앙에 두고 있던 있던 게 분명해요."

정답 : 1979년 <모두를 위한 정의>로 마지막 오스카 주연상 후보에 올랐었으나 이 영화를 제외하고는 70년대 말과 80년대 초를 통틀어 별로 신통한 영화에는 나오지 않았다.
출력 : 1979년 < 모두를 위한 정의 >로 마지막오스카주연상후보에 올랐었으나 이영화를 제외하고는 70년 대말과 80년 대초를 통틀어별로 신통한 영화에는 나오지 않았다.

정답 : 경찰은 처음에 그 사건이 아그자 혼자 저지른 단독 범행인 줄 알았지만 조사 결과 두 명 이상의 공범이 있었음이 밝혀졌고, 수사가 진전됨 에 따라 그 배후에는 여러 나라의 테러조직과 정보기관들이 난마처럼 얽혀 있음이 드러났다.
출력 : 경찰은 처음에 그 사건이 아 그 자 혼자저 지른 단독범행인 줄 알았지만 조사결과 두명이상의 공범이 있었음이 밝혀졌고, 수사가 진 전됨에 따라 그 배후에는 여러 나라의 테러 조직과 정보기관들이 난 마처럼 얽혀 있음이 드러났다.

Accuracy : 0.9272080232934325
```


## Performance Improvements

The adjustments to the SpacingRNN model have led to notable improvements in training performance, as demonstrated by the average cost reductions over epochs:

### Original Model Training Performance

```plaintext
Epoch 1: Average Cost = 0.617
Epoch 2: Average Cost = 0.324
Epoch 3: Average Cost = 0.243
Epoch 4: Average Cost = 0.211
Epoch 5: Average Cost = 0.190
Epoch 6: Average Cost = 0.174
Epoch 7: Average Cost = 0.160
Epoch 8: Average Cost = 0.152
Epoch 9: Average Cost = 0.142
Epoch 10: Average Cost = 0.131
```

### Enhanced Model Training Performance

```plaintext
Epoch 1: Average Cost = 0.661
Epoch 2: Average Cost = 0.474
Epoch 3: Average Cost = 0.224
Epoch 4: Average Cost = 0.187
Epoch 5: Average Cost = 0.165
Epoch 6: Average Cost = 0.153
Epoch 7: Average Cost = 0.140
Epoch 8: Average Cost = 0.131
Epoch 9: Average Cost = 0.125
Epoch 10: Average Cost = 0.116
Epoch 11: Average Cost = 0.112
Epoch 12: Average Cost = 0.107
Epoch 13: Average Cost = 0.101
Epoch 14: Average Cost = 0.097
Epoch 15: Average Cost = 0.094
```

### Enhanced Model Testing Performance

```plaintext
Original -> Accuracy : 0.9388721218906684
Enhanced -> Accuracy : 0.9272080232934325
```

## Additional Information for Google Colab Usage

To run this project on Google Colab, use the `rnn_spacing.ipynb` notebook file. This notebook is configured to work with Colab's environment and includes necessary setup steps for installing dependencies and running the model seamlessly in the cloud.

### Google Colab Results

Below are the screenshots from Google Colab demonstrating the training and testing phases of the SpacingRNN model:

#### Training Results on Google Colab
![Training Results](colab_result_images/train.jpg)

#### Testing Results on Google Colab
![Testing Results](colab_result_images/test.jpg)


## Acknowledgements
This project is based on the lecture materials provided by KUNLP. You can find the original materials at https://github.com/KUNLP/Lecture/tree/master/rnn/spacing. Special thanks to the contributors and authors of the lecture materials for their valuable insights and resources.

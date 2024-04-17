# SpacingRNN

This project implements a SpacingRNN model for Korean word spacing using PyTorch.

## Introduction

The SpacingRNN model aims to automatically insert spaces between Korean words in a sentence. This task is crucial for natural language processing applications such as text segmentation, parsing, and machine translation. The model utilizes a bidirectional LSTM (Long Short-Term Memory) neural network architecture to predict the optimal spacing between characters.

## Required Packages

To run this project, you need to have the following packages installed:

- `torch`: PyTorch deep learning framework.
- `numpy`: Numerical computing library for handling array operations.
- `scikit-learn`: Machine learning library for evaluating model performance.
- `os`: Operating system interfaces for accessing files and directories.

## Introduction to Data Files

### train.txt

The `train.txt` file is the training dataset used for training the SpacingRNN model. Each line consists of a Korean sentence without spaces and its corresponding spacing tags. The sentence and tags are separated by a tab.

### test.txt

The `test.txt` file is the test dataset used for evaluating the trained SpacingRNN model. It follows the same format as `train.txt`.

### eumjeol_vocab.txt

The `eumjeol_vocab.txt` file serves as the word vocabulary for Korean eumjeols (syllables). Each line represents a single eumjeol, with one eumjeol per line. This file is used when the model processes input data at the eumjeol level.

These files are utilized in training and evaluating the SpacingRNN model. The model is trained using the training data in `train.txt`, and its performance is evaluated using the test data in `test.txt`.
 

## Acknowledgements
This project is based on the lecture materials provided by KUNLP. You can find the original materials at https://github.com/KUNLP/Lecture/tree/master/rnn/spacing. Special thanks to the contributors and authors of the lecture materials for their valuable insights and resources.

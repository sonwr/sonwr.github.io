import torch
import torch.nn as nn

class MyLeNet5_1(nn.Module):
    def __init__(self):
        super(MyLeNet5_1, self).__init__()
        self.conv_1 = nn.Conv2d(1, 6, kernel_size = 5, padding=2)
        self.maxpool_1 = nn.MaxPool2d(kernel_size=(2,2), stride = 2)
        self.conv_2 = nn.Conv2d(6, 16, kernel_size = 5)
        self.maxpool_2 = nn.MaxPool2d(kernel_size=(2,2), stride = 2)
        self.conv_3 = nn.Conv2d(16, 120, kernel_size = 5)
        self.relu = nn.ReLU()
        self.fc_1 = nn.Linear(120, 84)
        self.fc_2 = nn.Linear(84, 10)

    def forward(self, x):
        x = self.conv_1(x)
        x = self.relu(x)
        x = self.maxpool_1(x)
        x = self.conv_2(x)
        x = self.relu(x)
        x = self.maxpool_2(x)
        x = self.conv_3(x)
        x = x.view(-1, 120) # flatten
        x = self.fc_1(x)
        x = self.relu(x)
        res = self.fc_2(x)
        return res

model = MyLeNet5_1()
print(model)

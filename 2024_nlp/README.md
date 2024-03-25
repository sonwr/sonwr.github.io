
# NLP Class Setup Guide

This guide will help you set up a Python environment for the NLP class using `pyenv`, `virtualenv`, and various essential Python packages.

## Step 1: Install Python Using pyenv

First, you need to install Python 3.9.5 using `pyenv`. If you haven't installed `pyenv` yet, refer to the [official pyenv installation guide](https://github.com/pyenv/pyenv#installation).

```bash
pyenv install 3.9.5
pyenv global 3.9.5
```

## Step 2: Upgrade pip and Install virtualenv

Upgrade `pip` to the latest version and install `virtualenv`:

```bash
pip install --upgrade pip
pip install virtualenv
```

## Step 3: Create and Activate a Virtual Environment

Create a virtual environment named `py395` and activate it:

```bash
virtualenv py395
source py395/bin/activate
```

## Step 4: Install Required Packages

Install the required Python packages for the NLP class:

```bash
pip install numpy scipy matplotlib
pip install jupyter
pip install scikit-learn
```

## Deactivating the Virtual Environment

When you're done working in the virtual environment, you can deactivate it by running:

```bash
deactivate
```

---


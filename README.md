Customer Churn Prediction & Analysis

Predict which customers are likely to churn and provide actionable analysis and visualizations.



🚀 Overview

This repository contains a customer churn prediction and analysis project built with Python. It includes data cleaning and EDA, feature engineering, several machine learning models (Logistic Regression, Random Forest, XGBoost/LightGBM), model evaluation, and a small demo web app (Streamlit) for live predictions and visualization of results.

The project is ideal for learning end-to-end data science workflows: from raw data to model training, evaluation, interpretation, and lightweight deployment.

🔎 Key Features

Exploratory Data Analysis (EDA) with visualizations

Data preprocessing and feature engineering pipeline

Multiple classification models with comparison

Model evaluation using accuracy, precision, recall, F1-score, ROC-AUC

SHAP or feature-importance based model explainability

Streamlit app for interactive prediction and dashboards

Jupyter notebooks for reproducibility and storytelling

🗂️ Dataset

This repo does not include any proprietary dataset. You can use public datasets such as:

Telco Customer Churn (Kaggle)

Any company-specific churn dataset (CSV with customer features + churn label)

Expected dataset format: a CSV with one row per customer and a binary target column named churn (0/1 or No/Yes). Adjust column names in the notebooks/scripts if yours differ.

Project Structure

├── data/
│   ├── raw/                # raw data files (not tracked)
│   └── processed/          # cleaned / feature-engineered CSVs
├── notebooks/
│   ├── 01_EDA.ipynb
│   ├── 02_FeatureEngineering.ipynb
│   └── 03_ModelTraining.ipynb
├── src/
│   ├── data.py             # data loading / preprocessing functions
│   ├── features.py         # feature engineering functions
│   ├── train.py            # training pipeline (CLI)
│   └── predict.py          # prediction helper functions
├── app/
│   └── app.py              # Streamlit demo app
├── requirements.txt
├── README.md
└── LICENSE

 Requirements

Create a virtual environment and install dependencies:

python -m venv .venv
source .venv/bin/activate   # macOS / Linux
.venv\Scripts\activate     # Windows PowerShell
pip install -r requirements.txt

Example requirements.txt:

pandas
numpy
scikit-learn
matplotlib
seaborn
xgboost
lightgbm
shap
streamlit
joblib
jupyterlab

🏁 Quick Start

Place your dataset in data/raw/churn_data.csv (or update the path in the notebooks).

Run the EDA notebook: notebooks/01_EDA.ipynb to explore data and visualizations.

Run feature engineering & preprocessing: notebooks/02_FeatureEngineering.ipynb.

Train models: notebooks/03_ModelTraining.ipynb or run training script:


Modeling & Evaluation

Try baseline Logistic Regression first to set a performance floor.

Train tree-based models (Random Forest, XGBoost, LightGBM) for better performance.

Use cross-validation and grid/random search for hyperparameter tuning.

Evaluate with confusion matrix, classification report (precision/recall/F1), and ROC-AUC.

Explainability

Use feature importances for tree models.

Use SHAP values for per-customer explanations and global feature impact.

📊 Visualizations

Include plots in the notebooks and Streamlit app:

Churn distribution, categorical distributions, correlation heatmap

ROC curve, precision-recall curve

Feature importance and SHAP summary plot

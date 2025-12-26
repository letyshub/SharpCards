# 🧠 Sharp Cards

Sharp Cards is a CLI (Command Line Interface) application for learning foreign languages using the **Spaced Repetition (Leitner System)** method. Built with .NET 8, it runs seamlessly on Windows, Linux, and macOS.

![Build Status](https://github.com/YOUR_USERNAME/Sharp Cards/actions/workflows/dotnet.yml/badge.svg)

## 🚀 Features

* **Leitner System:** Smart spaced repetition algorithm with 5 boxes. Words you remember well appear less frequently, optimizing your learning process.
* **Multiple Word Sets:** Organize your vocabulary into different categories (e.g., "Business English", "Spanish Basics") and switch between them easily.
* **Modern UI:** Features an interactive menu, colorful tables, and progress indicators powered by `Spectre.Console`.
* **CSV Import:** Quickly bulk import vocabulary lists from text files.
* **Cross-Platform:** Works on any operating system that supports .NET (Windows, Linux, macOS).

## 🛠️ Tech Stack

* **Language:** C# / .NET 8
* **UI Library:** Spectre.Console
* **Data Storage:** System.Text.Json (Local JSON file)
* **Testing:** xUnit

## 📥 Installation & Usage

1.  Ensure you have the [.NET 8 SDK](https://dotnet.microsoft.com/download) installed.
2.  Clone the repository:
    ```bash
    git clone [https://github.com/YOUR_USERNAME/Sharp Cards.git](https://github.com/YOUR_USERNAME/Sharp Cards.git)
    cd Sharp Cards
    ```
3.  Run the application:
    ```bash
    dotnet run --project src/VocabMaster.csproj
    ```
    *(Note: Replace `VocabMaster.csproj` with your actual project filename if you renamed it to `Sharp Cards.csproj`)*

## 🧪 Running Tests

The project includes unit tests that verify the logic of the spaced repetition algorithm (Leitner System). To run them:

```bash
dotnet test
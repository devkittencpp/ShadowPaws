# ShadowPaws SSH Control (v1.0.0)

ShadowPaws SSH Control is a cross-platform desktop application built with [Avalonia UI](https://avaloniaui.net/) that allows users to execute SSH commands remotely while managing custom macros and SSH settings via an intuitive graphical interface.

## Features

- **SSH Command Execution:**  
  Execute custom SSH commands with real-time output displayed in the application.

- **Macro Management:**  
  Create, edit, and delete macros that store frequently used SSH commands. Macros can be executed with a single click.

- **Dynamic UI:**  
  A modern, themed user interface with custom styling for buttons, text blocks, and text boxes.

- **SSH Settings:**  
  Easily configure SSH connection details (host, port, username, password) and test connections directly from the app.

- **JSON Configuration:**  
  Macros and SSH settings are stored in JSON files (`macros.json` and `config.json`), making it easy to back up or transfer your configurations.

## Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download) or later.
- [Avalonia UI](https://avaloniaui.net/) libraries.
- [Renci.SshNet](https://github.com/sshnet/SSH.NET) for SSH connectivity.
- [Newtonsoft.Json](https://www.newtonsoft.com/json) for JSON serialization and deserialization.

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/devkittencpp/ShadowPaws.git
cd ShadowPaws


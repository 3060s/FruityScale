<b>THIS README IS WIP SO IT WILL BE CHANGED SOON</b>

<a id="readme-top"></a>

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![License][license-shield]][license-url]

<br />
<div align="center">
  <a href="https://github.com/3060s/FruityScale">
    <img src="FruityScale.Infrastructure/Resources/favicon.png" alt="Logo" width="80" height="80">
  </a>

<h3 align="center">FruityScale</h3>

  <p align="center">
    A smart companion application that analyzes your FL Studio Piano Roll notes to instantly identify matching musical scales.
    <br />
    <a href="https://github.com/3060s/FruityScale"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/3060s/FruityScale">View Demo</a>
    &middot;
    <a href="https://github.com/3060s/FruityScale/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    &middot;
    <a href="https://github.com/3060s/FruityScale/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>

<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li><a href="#getting-started">Getting Started</a></li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
  </ol>
</details>

## About The Project

FruityScale is a desktop application designed to bridge the gap between your DAW and music theory. It currently integrates with FL Studio via a lightweight, internal script. 

With just few clicks, FruityScale retrieves note data directly from your current Piano Roll and instantly analyzes which musical scales those notes belong to. It is a quick and seamless way to stay in key, analyze complex chords, or figure out where to take your melodies next.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Built With

* [![C#][CSharp-shield]][CSharp-url]
* [![.NET][DotNet-shield]][DotNet-url]
* [![AvaloniaUI][Avalonia-shield]][Avalonia-url]

> **Note:** This project also utilizes **Serilog** for robust logging and **xUnit** for comprehensive unit testing.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Getting Started

### Prerequisites

To build this project from source, you need to install the required tools for your operating system:

#### Windows
1. **.NET 10 SDK** – Download and install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download).
2. **Git** – Download and install [Git for Windows](https://git-scm.com/download/win) (required to clone the repository via terminal).

#### macOS
1. **.NET 10 SDK** – Download and install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download) (select the installer matching your chip: Intel or Apple Silicon M1/M2/M3).
2. **Git** – Install via [Homebrew](https://brew.sh/) (`brew install git`) or download from [git-scm.com](https://git-scm.com/download/mac).

#### Linux
1. **.NET 10 SDK** – Install via your distribution's package manager (e.g., `sudo apt install dotnet-sdk-10.0` on Ubuntu) or follow the official [Linux .NET installation guide](https://learn.microsoft.com/en-us/dotnet/core/install/linux).
2. **Git** – Install via your package manager (e.g., `sudo apt install git` or `sudo dnf install git`).

---

### Installation & Building from Source

Once the prerequisites are installed, open your terminal (PowerShell or CMD on Windows) and follow these steps:

1. Clone the repository:
   ```sh
   git clone https://github.com/3060s/FruityScale.git

2. Navigate into the project directory:
   ```sh
   cd FruityScale

3. Restore dependencies
   ```sh
   dotnet restore

4. Run the application from source
   ```sh
   dotnet run --project FruityScale.App

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Visual Guide

Here you can see how to use export script inside FL Studio to generate .json file needed for app to match scales:

<div align="center">
  <img src="FruityScale.Infrastructure/Resources/FL_ExportGuide.gif" alt="FL Studio Export Guide" width="60%" height="60%" max-width="800px">
</div>

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Usage

FruityScale works side-by-side with FL Studio to make scale identification seamless. Follow these steps to analyze your project:

### 1. Prepare Your Notes
Open your project in FL Studio and write or record some notes inside the **Piano Roll**.

### 2. Run the Script in FL Studio
Execute the provided FruityScale internal script within FL Studio. This will save our notes from piano roll into notes.json file needed for our app (as shown in the [Visual Guide](#visual-guide)).

### 3. Fetch & Analyze
* Open **FruityScale** application.
* Click the **Scan Piano Roll** button on the main UI.

### 4. Explore the Results
FruityScale will instantly process your notes and provide a breakdown of:
* **Compatible scales:** List of musical scales that perfectly or closely match your note selection.
* **Played notes:** List of notes you have placed in piano roll.
* **Notes you can add:** List of notes you can add to your piano roll which match the selected scale.
* **Notes in selected scale:** List of notes available in selected scale.
* **Virtual keyboard:** Visual representation of keyboard with all things described above.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Roadmap

- [x] Initial FL Studio script integration
- [x] One-click note data fetching
- [x] Basic scale matching algorithm
- [ ] Support for other popular DAWs (Ableton, Logic Pro, Reaper, etc.)
- [ ] Advanced analysis tools and extended chord detection
- [ ] Experimental: Live MIDI input tracking with real-time scale detection

See the [open issues](https://github.com/3060s/FruityScale/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Top contributors:

<a href="https://github.com/3060s/FruityScale/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=3060s/FruityScale" alt="contrib.rocks image" />
</a>

## License

Distributed under the GPL-3.0 License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Contact

Project Link: [https://github.com/3060s/FruityScale](https://github.com/3060s/FruityScale)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

[contributors-shield]: https://img.shields.io/github/contributors/3060s/FruityScale.svg?style=for-the-badge
[contributors-url]: https://github.com/3060s/FruityScale/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/3060s/FruityScale.svg?style=for-the-badge
[forks-url]: https://github.com/3060s/FruityScale/network/members
[stars-shield]: https://img.shields.io/github/stars/3060s/FruityScale.svg?style=for-the-badge
[stars-url]: https://github.com/3060s/FruityScale/stargazers
[issues-shield]: https://img.shields.io/github/issues/3060s/FruityScale.svg?style=for-the-badge
[issues-url]: https://github.com/3060s/FruityScale/issues
[license-shield]: https://img.shields.io/github/license/3060s/FruityScale?style=for-the-badge
[license-url]: https://github.com/3060s/FruityScale/blob/main/LICENSE

[CSharp-shield]: https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white
[CSharp-url]: https://learn.microsoft.com/en-us/dotnet/csharp/
[DotNet-shield]: https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white
[DotNet-url]: https://dotnet.microsoft.com/
[Avalonia-shield]: https://img.shields.io/badge/AvaloniaUI-33A3E3?style=for-the-badge&logo=dotnet&logoColor=white
[Avalonia-url]: https://avaloniaui.net/

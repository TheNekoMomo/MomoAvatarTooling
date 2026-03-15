# Momo Avatar Tooling

![Unity](https://img.shields.io/badge/Unity-2022.3-black?logo=unity)
![VRChat](https://img.shields.io/badge/VRChat-SDK3-blue)
![Status](https://img.shields.io/badge/status-experimental-orange)

A Unity editor toolkit for improving **VRChat avatar workflows**.

This repository contains tools that help automate common avatar setup
tasks while developing a **larger system called MenuGraph**, which aims
to provide a fully visual workflow for building VRChat avatar menus and
related animator logic.

⚠️ **Important:**\
Most tools currently in this repository are **temporary utilities** and
may be **removed or replaced once MenuGraph is complete.**

------------------------------------------------------------------------

# Installing
You can install this package one of two ways both use the **VRChat Creator Companion (VCC)**
For easy install click this [LINK](https://thenekomomo.github.io/MomoAvatarTooling/) and follow the instructions.

The other method is as follows:
In **VCC**:
1. Open **Settings**
2. Go to **Packages**
3. Click **Add Repository**
4. Paste the URL below
```
https://TheNekoMomo.github.io/MomoAvatarTooling/index.json
```

------------------------------------------------------------------------

# Project Direction

The long-term goal of this repository is **MenuGraph**.

MenuGraph aims to provide:

-   A **node-based editor** for building VRChat avatar menus
-   Automatic **animator generation**
-   Automatic **parameter synchronization**
-   Simplified avatar workflow setup
-   Modular menu systems

Many of the tools currently included in this package exist to **solve
short-term workflow issues** while MenuGraph is under development.

------------------------------------------------------------------------

# Current Tools (Temporary)

These tools exist mainly to support current avatar workflows during
development.

They may change or disappear as MenuGraph evolves.

------------------------------------------------------------------------

## Sync VRChat Parameters → Animator

Utility for syncing parameters from a `VRCExpressionParameters` asset
into an Animator Controller.

Purpose: - Reduce manual setup errors - Ensure parameters exist in both
systems

Menu:

    Tools → Momo Avatar Toolkit → Sync VRChat Parameters to Animator

------------------------------------------------------------------------

## Fix Animator Transition Timing

Utility for standardizing Animator transition timing settings.

Menu:

    Tools → Momo Avatar Toolkit → Fix Animator Transitions

------------------------------------------------------------------------

# MenuGraph (Work in Progress)

MenuGraph is an experimental **graph-based workflow system** for
building VRChat avatar menus and related logic.

Goals include:

-   Visual node editing
-   Automatic menu generation
-   Automatic animator controller creation
-   Parameter management
-   Modular menu components

This system is **currently under active development and not yet
complete.**

------------------------------------------------------------------------

# Requirements

-   Unity **2022.3**
-   **VRChat SDK3 (Avatars)**
-   `com.unity.nuget.newtonsoft-json`

Make sure the VRChat SDK is installed before importing the package.

------------------------------------------------------------------------

# Installation

### Git URL

Open:

    Window → Package Manager

Then:

    + → Add package from git URL

Enter:

    git+https://github.com/<repo>/com.momo.avatar-tooling.git

------------------------------------------------------------------------

# Roadmap

Planned development focus:

-   MenuGraph editor
-   menu → animator generation
-   parameter management system
-   improved avatar workflow tooling

As MenuGraph becomes stable, many of the **temporary utilities will be
removed.**

------------------------------------------------------------------------

# License

This project uses a **custom license**.

See the `LICENSE` file in this repository for full details.

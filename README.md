# Modern UWP Designer

Modern UWP Designer is a Visual Studio extension that adds XAML Designer support for modern UWP .NET 9+ projects.

### Installation

[![Visual Studio Marketplace](https://img.shields.io/visual-studio-marketplace/v/AhmedWalid.ModernUwpDesigner?include_prereleases&label=Modern%20UWP%20Designer&style=for-the-badge&color=blue&link=https%3A%2F%2Fmarketplace.visualstudio.com%2Fitems%3FitemName%3DAhmedWalid.ModernUwpDesigner)](https://marketplace.visualstudio.com/items?itemName=AhmedWalid.ModernUwpDesigner)


### Current Limitations

- Currently only works on .NET 10+ projects (this limitation is temporary and will be resolved in future updates to expand the support to .NET 9 projects)
- Requires consumer projects to add reference to `Microsoft.Windows.CsWinRT` package version `2.3.0-prerelease.251115.2` or later
- Requires consumer projects to use `<WindowsSdkPackageVersion>10.0.xxxxx.73-preview</WindowsSdkPackageVersion>` (e.g. `<WindowsSdkPackageVersion>10.0.26100.73-preview</WindowsSdkPackageVersion>`) or later ***previews***
- Requires consumer application projects to have Publish Profiles configured and enabled
- Requires consumer application projects to have either `PublishAot` or `PublishSelfContained` enabled (they need to be set in both `.csproj` and `.pubxml` files)
- For application projects, only "Single Project MSIX" projects are supported, Windows App Packaging Projects (`wapproj`) are untested

### New Features

- `<SkipXamlDesignerSdkCheck>true</SkipXamlDesignerSdkCheck>` MSBuild property to skip the OS build check for the designer, allowing it to be used on older OS builds than the project SDK version

<img alt="image" src="https://github.com/user-attachments/assets/d99f9e55-aa38-4128-8657-d6e4d04fe517" />

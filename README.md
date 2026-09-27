# Hell Let Loose

Windows / .NET 8 project adapted for Hell Let Loose by Zach3rry59. The application targets `HLL-Win64-Shipping.exe` and includes a map view, an overlay, and Hell Let Loose map and actor definitions.

## Build

Requirements: Windows, the .NET 8 SDK (or a compatible newer SDK), and the project's native dependencies.

```powershell
dotnet build Hell-Let-Loose.sln -c Release -p:Platform=x64
```

Open `Hell-Let-Loose.sln` in Visual Studio to work on the project. The application is named **Hell Let Loose**; its assembly is `Hell-Let-Loose`, and its root namespace is `HellLetLoose`.

Publishing profiles write to `out/<Configuration>/` inside the checkout. Personal IDE settings and build output are ignored by Git.

## Dependencies

- FTD3XX.dll
- leechcore.dll, vmm.dll, dbghelp.dll, symsrv.dll, and vcruntime140.dll: [MemProcFS](https://github.com/ufrisk/MemProcFS/releases)
- SkiaSharp and the other NuGet packages declared in `Hell-Let-Loose.csproj`

## Project history and credits

This project originated from [Butter2222/squad-dma](https://github.com/Butter2222/squad-dma), through [Zach3rry59/squad-dma](https://github.com/Zach3rry59/squad-dma). The Git history is retained to preserve that provenance and the subsequent Hell Let Loose adaptation.

The original project credits x0m, Keegi, and MasterKeef, the [original EFT radar thread](https://www.unknowncheats.me/forum/escape-from-tarkov/482418-2d-map-dma-radar-wip.html), and [EFT-DMA-Radar-v2](https://www.unknowncheats.me/forum/escape-from-tarkov/639021-dma-radar-v2.html).

The old Squad mod-support list and screenshot are not presented as Hell Let Loose capabilities. Historical assets and commits remain in the repository for traceability.

## Validation scope

A successful build checks compilation and resource references. It does not establish compatibility with the current game version or validate runtime behavior with hardware attached.

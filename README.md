# osu-splitter

A program that allows you to split osu! beatmaps into multiple parts for practice purposes.

![osu! Beatmap Splitter](screenshots/osu-splitter.png)

## Download

https://github.com/cfxosu/osu-splitter

## Additional Notes

The application reads beatmap data directly from osu! memory while the game is running, making it incredibly responsive and accurate. You can split beatmaps into 2-100 parts based on hit object count, with automatic difficulty naming and .osz archive creation for easy import.

## Licenses

This project uses the following projects:

- [ProcessMemoryDataFinder](https://github.com/Piotrekol/ProcessMemoryDataFinder/tree/master) — licensed under [GPL-3.0](https://www.gnu.org/licenses/gpl-3.0.en.html)
- [OsuMemoryDataProvider](OsuMemoryDataProvider/) — custom implementation (included)
- [SF Pro Display fonts](assets/) — included in `assets/`
- [GDI+ (built into .NET Framework)](https://learn.microsoft.com/en-us/windows/win32/gdiplus/) (built into Windows/.NET)

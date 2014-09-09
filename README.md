Please note that the name of the servers cannot be displayed due to ArenaNet disabling the API after the transition to Megaservers.

I'm waiting the reactivation of the API to update the application.

**EDIT :** I used a cached file for getting server names

The files are hosted on a github repository : https://github.com/sidewinder94/GWvW_Overlay_Data

Please feel free to add your own files if you need other ones

Remarks
============
This application does not behave like regular overlays, hooking an application to display it's informations (like mumble overlay or overwolf), instead the windows are set to stay above every other window.

This behaviour prevents any instability due to the program.

Install
============
If you do not have the ability to compile the code, use the following binary (will be updated periodically)

- GWvW_Overlay https://dl.dropboxusercontent.com/u/42390504/gwvw/setup.exe
- .NET Framework 4.5 http://www.microsoft.com/en-us/download/details.aspx?id=30653

Update
============
You do not need to do anything for the application to update, it will check if an update is available and update itself when needed.

Usage
============
- Set Guild Wars 2's Resolution  in Graphic Options to Window Full-screen or Window mode.
- Launch the application
- Select appropriate match up
- Select specific borderlands
- Press "Home" to show/hide overlay map.

OR if you have a dual screen configuration :
Just drag and drop the two windows on your second screen and set the opacity to 100%

Changes
============
(05/04/2014)
- For the logitech Color LCD keyboards, it is now possible to change the current map using up & down keys

(30/03/2014)
- Added support for the Logitech G19 (should also work with every other Logitech keyboard with a color screen)
- Added cyclic navigation for the tabs of the color LCD applet

(29/03/2014)
- The owned time of the objectives tooltips now updates every seconds

(21/03/2014)
- Added localization support
- Added a tooltip including :
  - A timer for the current objective indicating the time since it is owned by the current faction
  - The guild owning the objective
- Some code cleaning
- French Localization

(29/10/2013)
- Fixed neutral icons for Ruins of Power
- Removed timers from Ruins of Power
- Fixed minor issues related to Ruins of Power

(17/9/2013)
- Replaced outdated borderlands map
- Added new icons for Bloodlust objectives

(02/7/2013)
- Added reset option for main window

(28/6/2013)
- Added objective counter
- Added About window

(18/6/2013)
- Added new interface for match selection
- Added automatic match selection based on pre-selected home server
- Fixed the bug that would cause crash when server name is "null"

(14/6/2013)
- Check for updates before application starts
- Changed color theme of tracker window

(13/6/2013)
- Fixed code that caused claims not to trigger
- Fixed disappearing tracker window

(12/6/2013)
- Added separate overlay for tracking events and timers on camp
- Re-done how click-trough is handled

(9/6/2013)
- Added separate options window
- Main window can now be resized
- Added guild claim icons
- Added objective names

(2/6/2013)
- Remapped show/hide shortcut to "Home" key
- Added map title
- Added opacity slider
- Updated Eternal Battlegrounds map
- Updated icons for objectives
- Enabled update feature

(1/6/2013)
- Added always on top checkbox
- Main window can now be dragged around
- Removed redundant function calls
- Fixed and issue with disappearing window
- Replaced the icon to avoid confusion with GW2.exe

(31/5/2013)
- Initial Release

Known Issues
============
- The timer indicating the time since an objective was owned will start at 0 for every map change, it is completely normal since the Anet provided API does not provide any way of knowing this. Instead i have to make a time difference since the objective's owner changed and the current time.
- Before the selection of a map after the selection of a server or a Match, the title of the map window may appear in a unlocalized version

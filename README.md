# SOD Building Upgrade Tool
A tool to upgrade sturctures and barricades without the need to salvage them first.
for example on vanilla maps, to upgrade pine floor to metal floor just get 10 metal then hit the floor with a hammer.

# How to use
- This module can run on any server. no rocketmod or other plugin platforms is needed.
- This module need 0Harmony library to be installed on the server. (you can search it on the internet)
- all what you need to do is to download the module from releases section, uncompress, copy the folder SodBuildingUpgrader to inside your server Modules folder. that's it. to confirm that you did it rigt the full path of the file Mod.module should look like this: Your_unturned_server/Modules/SodBuildingUpgrader/Mod.module
- if server map is not vanilla map, not Limestone, and not Arid Reborn then you should create config file for your map so the module get to know what to upgrade to what. you can see examples at config folder.

# Compile from source
if the release version is out dated or you want to complie the module by your self then at Visual Studion create a Classic Library (.Net framework) (.dll)
add the code files
add references to: UnityEngine.dll, UnityEngine.CoreModule.dll, Assembly-CSharp.dll, netstandard.dll, SDG.NetTransport.dll, 0Harmony.dll, com.rlabrecque.steamworks.net.dll
that's it. build the dll and update it at module folder.

# Credits
This tool were implemented by SOD.
the tool idea were given to us by Makus1me @ Arid Reborn.

# Support
feel free to contact us at SOD discord.

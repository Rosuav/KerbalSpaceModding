How hard can rocket modding be?!?
=================================

When you are navigating to a "below X meters" destination by flying horizontally,
it's easy enough to aim yourself, but much harder to estimate the time to your
target. This mod attempts to calculate this, with certain caveats:

* The celestial body is assumed to be spherical
* You are assumed to be traversing across the surface at your exact surface velocity
* You are assumed to be moving directly towards the destination unerringly
* Your velocity is assumed to be constant

Within those assumptions, this will provide time-to-destination every second.

Internal notes
--------------

https://wiki.kerbalspaceprogram.com/wiki/API:Vessel

Great example: https://github.com/taraniselsu/TacExamples/blob/main/01-SimplePartlessPlugin/Source/SimplePartlessPlugin.cs

$ tail -F ~/.config/unity3d/Squad/Kerbal\ Space\ Program/Player.log | grep ArmstrongNav

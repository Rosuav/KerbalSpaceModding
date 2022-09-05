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

$ tail -F ~/.config/unity3d/Squad/Kerbal\ Space\ Program/Player.log | grep '\[ArmstrongNav\]'


TODO: Whenever a maneuver node is created or moved, cite its altitude. Will allow height prediction
by just clicking on the path somewhere.


TODO: Autothrust. Entirely stateless and deterministic.
- If AT is active and there is no maneuver node, deactivate AT
- If AT is active and any time warp is happening, deactivate AT
- If AT is inactive and user clicks "Autothrust", activate AT and select mode "Wait"
- If mode is "Wait" and throttle is active, select mode "Burn"
- If mode is "Wait" and time to next maneuver is less than half than the burn time, activate throttle
- If mode is "Burn" and remaining burn time in maneuver is zero, deactivate throttle and AT
- If mode is "Burn" and maneuver node is more than X degrees away from us, deactivate throttle and AT
  - Need a way to catch the "end" of the burn in some way. Maybe if node is more than half its original
    burn time behind us??


Velocimeter
-----------

Guide your descent in the last phases.

* TODO: Calculate horizontal velocity (ground speed)

Depiction of velociraptor is taken from XKCD 135, made available under the terms of CC-BY-NC,
original image found here: https://xkcd.com/135/

![Velocimeter in use](VelocimeterScreenshot.png)

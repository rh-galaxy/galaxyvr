<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<html>
<head>
	<meta http-equiv="content-type" content="text/html; charset=iso-8859-1">
	<meta http-equiv="content-language" content="en-us">
	<meta name="description" content="Galaxy Forces VR - A space shooter with global hiscores and achievements.">
	<meta name="keywords" content="game VR space shooter hiscore achievements">
	<meta name="author" content="rh_galaxy">
	<meta name="viewport" content="initial-scale=1">

	<title>Galaxy Forces VR: Game</title>
	<link rel="stylesheet" type="text/css" href="gf.css">
	<link rel="icon" type="image/gif" href="images/favicon.gif">
</head>
<body>

<table class=main align="center" cellspacing="0" cellpadding="0">
<tr><td>

<br>
<?php
require_once("headline.php");
headline("Galaxy Forces VR");
?>
<br>

<?php
square_start("'100%'");
?>
<p>A single player space shooter game in VR. The player is in control of a ship.
The goal is to race against the clock to get the fastest time (<i>race</i>), or transport cargo to get the best score (<i>mission</i>).
All done in 55 narrow space caves filled with evil enemies. Featuring interactive music, realistic physics and challenging game-play.</p>

<p>The game is inspired by <a href="https://www.mobygames.com/game/amiga/gravity-force">Gravity Force</a> on the Amiga, and by <a href="http://www.galaxy-forces.com">Galaxy Forces V2</a>.</p>

<p>All players may have their best score for each level sent to this website and the top scores and top players are displayed below. The replay of the record scores are possible to view from the game and here to see how it was done. There are also 22 achievements to unlock.</p>

<p>Available for free on <a href="https://www.oculus.com/experiences/rift/2005558116207772/">Oculus Home</a> (for Rift), and <a href="https://store.steampowered.com/app/1035550/Galaxy_Forces_VR/">Steam</a> (Rift, Vive and others) since 20th of September 2019.</p>
<p>Also free on <a href="https://www.oculus.com/experiences/quest/4116487761695377/">AppLab</a> (<a href="https://sidequestvr.com/app/2058/galaxy-forces-vr">SideQuest</a> page).</p>
<p><video width="860" height="484" controls poster="/images/screen4_race13_860.jpg"><source src="gfvr16.mp4" type="video/mp4">Your browser does not support the video tag.</video></p>
<p><img align="left" src="images/screen8_860.jpg" title="Galaxy Forces VR screenshot"></p>

<table cellspacing="10" cellpadding="0">
<tr>
</tr>
</table>
<?php
square_end();
?>
<br><br><br>


<?php
headline("Level editor");
?>
<br>
<?php
square_start("'100%'");
?>
<p>The ability to create and play custom levels is now added. Use the MapEditor for <a href="MapEditor_Win_x86.zip">Windows</a>, <a href="MapEditor_Mac_x64.zip">Mac</a> or <a href="MapEditor_Linux_x64.zip">Linux</a> and read editor_readme.txt</p>
<p>In the <a href="https://discord.gg/cjptxT5JCb">Discord server</a> there is a channel for sharing levels with me and others and I can add them to the games user level page for approved levels.</p>
<?php
square_end();
?>
<br><br><br>


<?php
headline("Public Domain");
?>
<br>
<?php
square_start("'100%'");
?>
<p>Since November 2020 the Unity projects source for all versions are available on <a href="https://sourceforge.net/projects/galaxy-forces-vr/">SourceForge</a>. And on <a href="https://github.com/rh-galaxy/galaxyvr">GitHub</a>.</p>
<p>If you find the source useful I'd like to hear from you.</p>
<?php
square_end();
?>
<br><br><br>


<a name="hiscore"></a>
<?php
headline("Hiscores PC");
?>
<br>

<?php
square_start("'100%'");
?>
<p>Steam users are prefixed 's_' to separate them from Oculus users (no prefix).</p>
<table width="95%">
<tr><td>
<?php
require_once("print_hiscore2.php");
print_hiscore();
?>
</td></tr>
</table>
<?php
square_end();
?>
<br><br><br>


<a name="hiscore2"></a>
<?php
headline("Hiscores Quest");
?>
<br>

<?php
square_start("'100%'");
?>
<table width="95%">
<tr><td>
<?php
require_once("print_hiscore2_quest.php");
print_hiscore_quest();
?>
</td></tr>
</table>
<?php
square_end();
?>
<br><br><br>


<a name="hiscore3"></a>
<?php
headline("Hiscores JioGlass");
?>
<br>

<?php
square_start("'100%'");
?>
<table width="95%">
<tr><td>
<?php
require_once("print_hiscore2_jio.php");
print_hiscore_jio();
?>
</td></tr>
</table>
<?php
square_end();
?>
<br><br><br>


<a name="hiscore4"></a>
<?php
headline("Hiscores PC no VR");
?>
<br>

<?php
square_start("'100%'");
?>
<table width="95%">
<tr><td>
<?php
require_once("print_hiscore2_pc.php");
print_hiscore_pc();
?>
</td></tr>
</table>
<?php
square_end();
?>
<br><br><br>


<?php
headline("Future and past");
?>
<br>
<?php
square_start("'100%'");
?>
<p>Point-movement (all platforms v1.40) where you point with your hand and the ship goes there.
The plan is to also add multiplayer for 4 players with 4 new dogfight maps as well as the existing race and mission maps. Race will be done without collision between players, and mission as co-op where you cooperate to transport all cargo.</p>
<p>v1.96 Feb 25, 2023<br>
- PC noVR versions on itch.io with hiscore and progress<br>
- fix ship sometimes exploding at start<br>
- fix sorting order (cursor and status bar)<br>
<br>
v1.95 Nov 9, 2022<br>
- add swinging cargo mode<br>
<br>
v1.90 Sep 6, 2022<br>
- use universal render pipeline (PC)<br>
- add glow postprocessing (PC)<br>
- replaced skyboxes<br>
- replaced planet<br>
- removed recentering<br>
- add adjust height (Y) and front (Z)<br>
- major update to build env<br>
<br>
v1.86 Jul 29, 2022<br>
- controls-info update<br>
- collision stuck fix<br>
- door base collision fix<br>
- auto land with point motion<br>
- auto fire with point motion<br>
- not instant kill on stationary enemies<br>
<br>
v1.80 Jun 7, 2021<br>
- new user levels<br>
- recenter bugfix<br>
- update FMOD and SteamWorks.NET<br>
- level editor, max 10wp<br>
<br>
v1.75 May 12, 2021<br>
- fix EyeOfTheStorm<br>
- fade in is 1sec shorter<br>
- optimize radiotower<br>
- fixed house and hangar<br>
- optimized toxic barrels<br>
- bugfix duplicate enemy kill<br>
- support 4*10 user levels<br>
- ship status, mipmap fix<br>
<br>
v1.70 Apr 23, 2021<br>
- race08, race22 optimized<br>
- choppyness fixed<br>
- add skyview (quest)<br>
<br>
v1.65 Apr 10, 2021<br>
- gui selection bugfix<br>
- add linux editor<br>
<br>
v1.57 Mar 24, 2021<br>
- add gui marker for Easy mode<br>
- optimized LandingZone.mat<br>
- fix errors when no steamvrinput connected<br>
- CameraHolder singleton fix<br>
- add best score<br>
- add achievement 22<br>
<br>
v1.53 Jan 19, 2021<br>
- add less than bronze indication (green)<br>
- Easy mode implemented<br>
- custom levels page<br>
- crashfix<br>
<br>
v1.48 Nov 7, 2020<br>
- first quest version<br>
- explosion fix<br>
- radiotower perf fix<br>
- point in menu<br>
- fix menu selection bug<br>
- install size 1/3<br>
- lower engine vol<br>
- bigger status box<br>
<br>
v1.44 May 9, 2020<br>
- fix flying particle fx<br>
- bugfix in first menu level selection<br>
- add enemy-kill msg to replay<br>
- fix max steam user name length<br>
- fix web score spacing<br>
- fix steam gold achievement<br>
- frame rate fix in CollisionStay2D<br>
- point motion implemented<br>
<br>
v1.26 Dec 1, 2019<br>
- add flying score text<br>
- hiscore security increased<br>
- move game status up<br>
- major fix: replays working<br>
<br>
v1.20 Aug 10, 2019<br>
- move view up 20cm<br>
- fix menu options positions<br>
- fix for distance achievement<br>
- changed controller bindings for recenter and back<br>
- add recenter action, only in menu (for now)<br>
<br>
v1.11 Jul 28, 2019<br>
- add gold, silver and bronze limits to menu<br>
- tracking reset button in menu<br>
- game status position fix. snap movement fix<br>
- shadows, credits, controls<br>
- bugfixes to camera position and damage taken<br>
- 6DoF dollhouse impl finished<br>
<br>
v1.00 Jul 14, 2019<br>
- add two new achievements<br>
- fix cargo sound<br>
- music: always exit flow on death<br>
- music: fixed transitions<br>
- music: added EnemyVol parameter<br>
- music: changed death filter<br>
- music: restarts sooner after death<br>
- mission music feature when enemies/bullets are nearby<br>
- pitch variation on death sweep<br>
- small drum sounds occur intermittently with higher cargo<br>
</p>
<?php
square_end();
?>
<br><br><br>

<?php
headline("Multiplayer coders");
?>
<br>
<?php
square_start("'100%'");
?>
<p>Contact me if you are interested in collaborating to get this vr game feature done.
I have set a time frame to 8 months, and have more info of what is needed. Join the <a href="https://discord.gg/cjptxT5JCb">Discord server</a> for this purpose</p>
<?php
square_end();
?>
<br><br><br>

<?php
headline("Contact");
?>
<br>
<?php
square_start("'100%'");
?>
<p>contact@galaxy-forces-vr.com</p>
<?php
square_end();
?>
<br>

</td></tr>
</table>

<?php
	//update statistics
	require_once("db_connect.php");
	$db = connect_to_db();
	$query = "UPDATE statistics_t SET count=count+1 WHERE page='index.php'";
	$info = mysqli_query($db, $query);
	if($db) mysqli_close($db);
?>

</body>
</html>
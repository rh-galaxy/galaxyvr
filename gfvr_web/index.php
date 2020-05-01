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
All done in 55 narrow space caves filled with enemies. Featuring interactive music, realistic physics and challenging game-play.</p>

<p>The game is inspired by <a href="https://www.mobygames.com/game/amiga/gravity-force">Gravity Force</a> on the Amiga, and by <a href="http://www.galaxy-forces.com">Galaxy Forces V2</a>.</p>

<p>All players may have their best score for each level sent to this website and the top scores and top players are displayed below. The replay of the record scores are possible to view from the game to see how it was done. There are also 21 achievements to unlock.</p>

<p>Available on <a href="https://www.oculus.com/experiences/rift/2005558116207772/">Oculus Home</a> (for Rift), and <a href="https://store.steampowered.com/app/1035550/Galaxy_Forces_VR/">Steam</a> (Rift, Vive and others) since 20th of September 2019.</p>
<p><img align="left" src="images/screen8_860.jpg" title="Galaxy Forces VR screenshot"></p>
<p><video width="860" height="484" controls poster="/images/screen4_race13_860.jpg"><source src="gfvr16.mp4" type="video/mp4">Your browser does not support the video tag.</video></p>

<table cellspacing="10" cellpadding="0">
<tr>
</tr>
</table>
<?php
square_end();
?>
<br><br><br>

<a name="hiscore"></a>
<?php
headline("Hiscores");
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


<?php
headline("Level editor");
?>
<br>
<?php
square_start("'100%'");
?>
<p>The ability to create and play custom levels is now added. Use the <a href="MapEditor_for_custom_levels.zip">MapEditor</a> and read editor_readme.txt</p>
<?php
square_end();
?>
<br><br><br>


<?php
headline("Future");
?>
<br>
<?php
square_start("'100%'");
?>
<p>Point-movement (now done for both Steam and Oculus v1.40) where you point with your hand and the ship goes there.
The plan is to also release it for the Oculus Go.</p>
<?php
square_end();
?>
<br><br><br>

<?php
headline("Contest 2020-01-19");
?>
<br>
<?php
square_start("'100%'");
?>
<p>A new contest! $55 via paypal to the one player at the top of the hiscores listing at the end of January 19, 2020 CET.
I want to get the hiscores flowing with people trying and in that case somebody will win, but if it's only a single person trying, I will also be trying to better my score (rh_galaxy).</p>
<p>The winner is 'Mahed', please make contact on mail address below.</p>
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
<p>Since April 09 2020 the Unity projects source for both the Oculus and Steam versions are available on <a href="https://sourceforge.net/projects/galaxy-forces-vr/">SourceForge</a>.</p>
<p>If you find the source useful I'd like to hear from you.</p>
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
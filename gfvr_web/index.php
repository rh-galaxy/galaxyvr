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

<p>All players may have their best score for each level sent to this website and the top scores and top players are displayed below. The replay of the record scores are possible to view from the game to see how it was done. There are also 19 achievements to unlock.</p>

<p>Available on Oculus Home (for Rift), and Steam (Rift and Vive) at 14th of June 2019 as a preliminary date.</p>
<p><img align="left" src="images/screen8_860.jpg" title="Galaxy Forces VR screenshot"></p>
<p><img align="left" src="images/screen7_860.jpg" title="Galaxy Forces VR screenshot"></p>

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
<table width="83%">
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
headline("Future");
?>
<br>
<?php
square_start("'100%'");
?>
<p>If there is interest, the ability to create and play custom levels will be added later. 
And there may be more official levels with hiscore coming. After the release on Vive and Rift,
the plan is to distribute it for the Oculus Quest.</p>
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
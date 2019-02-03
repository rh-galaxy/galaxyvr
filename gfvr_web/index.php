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
<p>A single player space shooter game in VR (Oculus Rift). The player is in control of a ship.
The goal is to race against the clock to get the fastest time (<i>race</i>), or transport cargo to get the best score (<i>mission</i>).</p>

<p>There will be more than 50 levels when this game is ready for release some time later this year.</p>

<p>The game is inspired by <a href="https://www.mobygames.com/game/amiga/gravity-force">Gravity Force</a> on the Amiga, and by <a href="http://www.galaxy-forces.com">Galaxy Forces V2</a>.</p>

<p>All players may have their best score for each level sent to this website and the top scores and top players are displayed below. The replay of the record scores are possible to view from the game to see how it was done.</p>

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
headline("Want to help?");
?>
<br>
<?php
square_start("'100%'");
?>

<h4>Contact</h4>
<p>contact@galaxy-forces-vr.com</p>
<h4>Early development screenshot</h4>
<p><img align="left" src="images/early_dev_screenshot1_860.jpg" title="Galaxy VR early dev"></p>
<h4>Early development screenshot, head pitch down and turned to the left, greater fov</h4>
<p><img align="left" src="images/early_dev_screenshot2_860.jpg" title="Galaxy VR early dev"></p>
<h4>Early development demo</h4>
<p><a href="http://www.galaxy-forces-vr.com/GalaxyForcesVR_v0.3.zip">Galaxy Forces VR v0.3</a></p>

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
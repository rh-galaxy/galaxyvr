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
	<!-- Facebook Pixel Code -->
	<script>
	  !function(f,b,e,v,n,t,s)
	  {if(f.fbq)return;n=f.fbq=function(){n.callMethod?
	  n.callMethod.apply(n,arguments):n.queue.push(arguments)};
	  if(!f._fbq)f._fbq=n;n.push=n;n.loaded=!0;n.version='2.0';
	  n.queue=[];t=b.createElement(e);t.async=!0;
	  t.src=v;s=b.getElementsByTagName(e)[0];
	  s.parentNode.insertBefore(t,s)}(window, document,'script',
	  'https://connect.facebook.net/en_US/fbevents.js');
	  fbq('init', '2387909674774682');
	  fbq('track', 'PageView');
	</script>
	<noscript><img height="1" width="1" style="display:none"
	  src="https://www.facebook.com/tr?id=2387909674774682&ev=PageView&noscript=1"
	/></noscript>
	<!-- End Facebook Pixel Code -->
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

<p>Available on Oculus Home (for Rift), and Steam (Rift and Vive) at 26th of July 2019 as a preliminary date.</p>
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
<p>There may be more official levels with hiscore coming.
The plan is to also release it for the Oculus Quest.</p>
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
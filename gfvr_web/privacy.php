<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<html>
<head>
	<meta http-equiv="content-type" content="text/html; charset=iso-8859-1">
	<meta http-equiv="content-language" content="en-us">
	<meta name="description" content="Galaxy Forces VR - Privacy Policy">
	<meta name="keywords" content="game VR space shooter hiscore achievements">
	<meta name="author" content="rh_galaxy">
	<meta name="viewport" content="initial-scale=1">

	<title>Galaxy Forces VR: Privacy</title>
	<link rel="stylesheet" type="text/css" href="gf.css">
	<link rel="icon" type="image/gif" href="images/favicon.gif">
</head>
<body>

<table class=main align="center" cellspacing="0" cellpadding="0">
<tr><td>
<br>

<?php
require_once("headline.php");
headline("Galaxy Forces VR - Privacy Policy");
?>
<br>
<?php
square_start("'100%'");
?>
<p><i>Team Galaxy</i> is committed to protecting the privacy of players, although providing online hi-scores discloses your user name of the platform your game is playing on. This privacy policy describes how we use and protect any information that you give us. We may change this policy from time to time by updating this page.</p>

<p>We may collect the following information:<br>
<i>Your location (country).</i><br>
<i>The type of device you’re using.</i><br>
<i>Your game play statistics - disclosed on this website.</i>
</p>
<p>We collect this information to provide you with a good achievement system, to keep you interested in the game.</p>
<p>We are committed to ensuring that your information is secure. In order to prevent unauthorized access or disclosure we have put in place suitable physical, electronic and managerial procedures to safeguard and secure the information we collect online.</p>
<p>We will not send you an email if you don't send us one first. No other means of communication will be used. Mail to contact@galaxy-forces-vr.com.</p>
<p>We will not sell, distribute or lease your personal information to third parties, unless we are required by law to do so.</p>
<p>Under the data protection act 1998 you may request details of any personal information that we may have on you. If you believe that any information we are holding is incorrect or incomplete, please email us. We will correct any information found to be incorrect or delete such information.</p>


<?php
square_end();
?>

</td></tr>
</table>

<?php
	//update statistics
	require_once("db_connect.php");
	$db = connect_to_db();
	$query = "UPDATE statistics_t SET count=count+1 WHERE page='privacy.php'";
	$info = mysqli_query($db, $query);
	if($db) mysqli_close($db);
?>

</body>
</html>

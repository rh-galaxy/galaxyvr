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
<p><i>Team Galaxy</i> collect and store the following information:<br>
<i>USER_ID, USER_PROFILE (name only).</i><br>
<i>The data is used to keep track of your game play progress and high scores - disclosed on this web-site and in the game. For the best score for each level that you complete a recorded replay is stored, viewable by others.</i>
</p>
<p>We will not distribute your personal information to third parties, unless we are required by law to do so.</p>
<p>You can request to delete this data by sending an email to contact@galaxy-forces-vr.com
</p>


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

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<html>
<head>
	<meta http-equiv="content-type" content="text/html; charset=iso-8859-1">
	<meta http-equiv="content-language" content="en-us">
	<meta name="description" content="Galaxy Forces VR - A space shooter with global hi-scores and achievements.">
	<meta name="keywords" content="hi-score top-list achievements">
	<meta name="author" content="Ronnie Hedlund">

	<title>Galaxy Forces V2: Level achievements</title>
	<link rel="stylesheet" type="text/css" href="gf.css">
	<link rel="icon" type="image/gif" href="images/favicon.gif">
	
	<script type="text/javascript" src="utf8_encode.js"></script>
	<script type="text/javascript" src="md5.js"></script>
</head>
<body>

<table class=main align="center" cellspacing="0" cellpadding="0">
<tr><td>

<br>
<?php
require_once("headline.php");
headline("Level achievements");
?>
<br>

<?php
square_start("'100%'");
?>


<?php

	require_once("db_connect.php");
	require_once("checkparams.php");
	
	$level = $_GET["Level"];
	$paramArray = array($level);
	if(checkstring($paramArray)) {
		echo "<h3>Score for ".$level." (on PC)</h3>";
	
		$db = connect_to_db();
		if($db) {

			$select_string = "SELECT is_time,limit1,limit2,limit3 FROM levels_t WHERE levels_t.level='".$level."'";
			$result = @mysqli_query($db, $select_string);
			$row = @mysqli_fetch_assoc($result);

			$is_time = $row['is_time'];
			$sort = "DESC";
			if($is_time != 0) {
				$sort = "ASC";
			}
			$limit1 = $row['limit1'];
			$limit2 = $row['limit2'];
			$limit3 = $row['limit3'];
			
			$select_string = "SELECT members_t.username, achievements_t.score AS score FROM members_t, achievements_t WHERE members_t.oculus_id=achievements_t.user_id AND achievements_t.level='".$level."' ORDER BY achievements_t.score ".$sort." LIMIT 0,99";
			// make query
			$result = @mysqli_query($db, $select_string);
			// succeeded
			if($result) {

				// count rows
				$num_rows = @mysqli_num_rows($result);
				// it better be more than 0
				if($num_rows > 0) {

					echo "<table class=data width='75%' cellspacing=2 cellpadding=0><tr>".
						"<td width='55%'><span class='colored2'>Name</span></td><td></td><td><span class='colored2'>Score</span></td></tr>";
					for($i=0; $i < $num_rows; $i++) {
						$row = @mysqli_fetch_assoc($result);
					
						//name						
						echo "<tr><td>".$row["username"]."</td>";
						
						//icon
						$icon = "-";
						if($is_time != 0) {
							if($row["score"]<$limit1) $icon = "gold.png";
							else if($row["score"]<$limit2) $icon = "silver.png";
							else if($row["score"]<$limit3) $icon = "bronze.png";
							else $icon = "green.png";
						} else {
							if($row["score"]>$limit1) $icon = "gold.png";
							else if($row["score"]>$limit2) $icon = "silver.png";
							else if($row["score"]>$limit3) $icon = "bronze.png";
							else $icon = "green.png";
						}
						echo "<td><img align=\"left\" src=\"images/a_".$icon."\" title=\"".$icon."\" alt=\"".$icon."\"></td>";
						
						//score
						echo "<td>";
						if($is_time != 0) {
							// convert to time
							$remain = $row["score"];
							$minutes = (int)($remain/(1000*60));
							$remain -= $minutes*1000*60;
							$seconds = (int)($remain/1000);
							$remain -= $seconds*1000;

							if($minutes < 60) {
								printf("%02d:%02d.%02d", $minutes, $seconds, $remain/10);
							} else {
								echo "00:00.00";
							}
						} else {
							// normal score
							$remain = $row["score"];
							$part1 = (int)($remain/1000);
							$remain -= $part1*1000;
							if($remain<0) $remain *= -1;
							printf("%d.%02d", $part1, $remain/10);
						}
						echo "</td></tr>";
					}
					echo "</table>";
				}
			}
			mysqli_close($db);
		}
	}
?>

<?php
square_end();
?>
<br><br><br>

</td></tr></table>
</body>
</html>
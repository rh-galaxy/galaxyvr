<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<html>
<head>
	<meta http-equiv="content-type" content="text/html; charset=iso-8859-1">
	<meta http-equiv="content-language" content="en-us">
	<meta name="description" content="Galaxy Forces VR - A space shooter with global hi-scores and achievements.">
	<meta name="keywords" content="hi-score top-list achievements">
	<meta name="author" content="Ronnie Hedlund">

	<title>Galaxy Forces V2: User achievements</title>
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
headline("User achievements");
?>
<br>

<?php
square_start("'100%'");
?>


<?php

	require_once("db_connect_quest.php");
	require_once("checkparams.php");
	
	$name = $_GET["Name"];
	$id = $_GET["Id"];
	$paramArray = array($name,$id);
	if(checkstring($paramArray)) {
		echo "<h3>Score for ".$name." (on Quest)</h3>";
	
		$db = connect_to_db_quest();
		if($db) {
		
			$select_string = "SELECT * FROM levels_t ORDER BY levels_t.ordering ASC";
			// make query
			$result = @mysqli_query($db, $select_string);
			// succeeded
			if($result) {
				// count rows
				$num_rows = @mysqli_num_rows($result);
				// it better be more than 0
				if($num_rows > 0) {
				
					echo "<table class=data width='49%' cellspacing=2 cellpadding=0><tr>".
						"<td width='40%'><span class='colored2'>Level</span></td><td></td><td><span class='colored2'>Score</span></td></tr>";
					for($i=0; $i < $num_rows; $i++) {
						$row = @mysqli_fetch_assoc($result);
					
						//should only result in one row (or 0)
						$select_string = "SELECT achievements_t.level AS Level, achievements_t.score AS Score, levels_t.is_time AS is_time, levels_t.limit1 AS limit1, levels_t.limit2 AS limit2, levels_t.limit3 AS limit3".
							" FROM achievements_t, levels_t WHERE achievements_t.level=levels_t.level AND achievements_t.level='".$row['level']."' AND achievements_t.user_id='".$id."'";
					
						$result2 = @mysqli_query($db, $select_string);

						//level						
						echo "<tr><td>".$row["level"]."</td>";
						
						// succeeded
						if($result2) {
							$num_rows2 = @mysqli_num_rows($result2);
							if($num_rows2 == 0) {
								echo "<td>-</td><td>-</td></tr>";
							} else if($num_rows2 > 0) {
								// one value
								$row2 = @mysqli_fetch_assoc($result2);

								$is_time = $row2["is_time"];

								//icon
								$icon = "-";
								if($is_time != 0) {
									if($row2["Score"]<$row2["limit1"]) $icon = "gold.png";
									else if($row2["Score"]<$row2["limit2"]) $icon = "silver.png";
									else if($row2["Score"]<$row2["limit3"]) $icon = "bronze.png";
									else $icon = "green.png";
								} else {
									if($row2["Score"]>$row2["limit1"]) $icon = "gold.png";
									else if($row2["Score"]>$row2["limit2"]) $icon = "silver.png";
									else if($row2["Score"]>$row2["limit3"]) $icon = "bronze.png";
									else $icon = "green.png";
								}
								echo "<td><img align=\"left\" src=\"images/a_".$icon."\" title=\"".$icon."\" alt=\"".$icon."\"></td>";
								
								//score
								echo "<td>";
								
								$prefix = "1";
								if($is_time != 0) $prefix = "2";
								if($i > 54) $prefix = "";
								echo "<a href=\"webreplay/index.html?Level=".$prefix.$row['level']."&amp;Id=".$id."&amp;IsQuest=1\">";
								
								if($is_time != 0) {
									// convert to time
									$remain = $row2["Score"];
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
									$remain = $row2["Score"];
									$part1 = (int)($remain/1000);
									$remain -= $part1*1000;
									if($remain<0) $remain *= -1;
									printf("%d.%02d", $part1, $remain/10);
								}
								echo "</a></td></tr>";
							}
						} else {
							//should never happen
							echo "<td>-</td><td>-</td></tr>";
						}
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
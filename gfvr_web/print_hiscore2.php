<?php

function print_hiscore()
{
	require_once("db_connect.php");

	$db = connect_to_db();

	if($db) {

		$select_string = "SELECT scr_t.name, SUM(scr_t.pos_score) AS score_sum FROM".
			" (SELECT achievements_t.name AS name, achievements_t.level, achievements_t.score, levels_t.is_time, levels_t.limit3, levels_t.limit3 -achievements_t.score AS pos_score FROM achievements_t, levels_t WHERE achievements_t.level = levels_t.level AND levels_t.is_time=1".
			" UNION ALL".
			" SELECT achievements_t.name AS name, achievements_t.level, achievements_t.score, levels_t.is_time, levels_t.limit3, achievements_t.score AS pos_score FROM achievements_t, levels_t WHERE achievements_t.level = levels_t.level AND levels_t.is_time=0) scr_t".
			" GROUP BY scr_t.name".
			" ORDER BY score_sum DESC LIMIT 0,51";

		$result = @mysqli_query($db, $select_string);
		
		// succeeded
		if($result) {
			
			// count rows
			$num_rows = @mysqli_num_rows($result);
			// it better be more than 0
			if($num_rows > 0) {
			
				echo "<p>Top 51 players</p>";
			
				$num_per_table = 17;
				for ($j=0; $j < floor(($num_rows+($num_per_table-1))/$num_per_table); $j++) {
					// table header
					echo "<table class=ranking width='31%' cellspacing=0 cellpadding=0>";
					echo "<tr><td></td><td><span class='colored2'>Name</span></td><td><span class='colored2'>Rank</span></td></tr>";
					
					$to = $num_per_table;
					for($i=$j*$num_per_table; $i < $num_rows; $i++) {
						$row = @mysqli_fetch_assoc($result);

						echo "<tr>";
						
						echo "<td><i>";
						echo ($i+1)."&nbsp;";
						echo "</i></td>";
						
						echo "<td><a href=\"print_hiscore_for_user.php?Name=".$row["name"]."\">";
						echo $row["name"];
						echo "</a></td>";
						
						echo "<td>";
						printf("%d.%d", $row["score_sum"]/1000, ($row["score_sum"]%1000)/10);
						echo "</td>";
						echo "</tr>";
					
						if(--$to == 0) break;
					}
					// table end
					echo "</table>";

				}
			}
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////
		$select_string = "SELECT levels_t.level AS Level, levels_t.is_time AS IsTime, levels_t.is_singleplayer AS IsSP FROM levels_t ORDER BY levels_t.is_time DESC, levels_t.ordering ASC";
		// make query
		$result = @mysqli_query($db, $select_string);
		// succeeded
		if($result) {
			// table header
			echo "<table class=data width='100%' cellspacing=2 cellpadding=0>";
			echo "<tr><td width='22%'></td><td></td><td></td> <td></td><td width='18%'></td></tr>";
			echo "<tr><td></td><td><span class='colored2'>1st</span></td><td><span class='colored2'>2nd</span></td><td><span class='colored2'>3rd</span></td><td></td><td></td></tr>";

			// count rows
			$num_rows = @mysqli_num_rows($result);
			// it better be more than 0
			if($num_rows > 0) {
				for($i=0; $i < $num_rows; $i++) {
					$row = @mysqli_fetch_assoc($result);
					$sort = "DESC";
					if($row["IsTime"] != 0) $sort = "ASC";
					$select_string  = "SELECT achievements_t.name AS Name, achievements_t.score AS Score FROM achievements_t WHERE achievements_t.level='".$row['Level']."' ORDER BY achievements_t.score ".$sort." LIMIT 0,3";
					$result2 = @mysqli_query($db, $select_string);


					echo "<tr><td><i>";
					//if($row["IsSP"]>0) echo "<a href=\"web_play.php?Level=".$row["Level"]."&amp;Name=You\">";
					echo $row["Level"];
					//if($row["IsSP"]>0) echo "</a>";
					echo "</i></td>";


					$num_rows2 = 0;
					if($result2) {
						$num_rows2 = @mysqli_num_rows($result2);
						for($j=0; $j < $num_rows2; $j++) {
							// a bit slower but easier to read
							$row2 = @mysqli_fetch_assoc($result2);

							//name
							echo "<td>";
							echo $row2["Name"];
							echo "<br>";

							// score
							//echo "<a href=\"web_replay2.php?Level=".$row["Level"]."&amp;Name=".$row2["Name"]."\">";

							if($row["IsTime"] != 0) {
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
							//echo "</a></td>";
							echo "</td>";
						}
					}
					for($j=0; $j < 3-$num_rows2; $j++) {
						echo "<td></td>";
					}
					echo "<td></td>";
					echo "<td></td>";

					echo "</tr>";
				}
			}
			// table end
			echo "</table>";
		}

		mysqli_close($db);
	}
}

?>

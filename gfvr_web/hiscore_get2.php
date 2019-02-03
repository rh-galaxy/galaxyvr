<?php

	require_once("db_connect.php");
	require_once("checkparams.php");
	
	$name = $_GET["Name"];
	$paramArray = array($user);
	if(checkstring($paramArray)) {

	
		$db = connect_to_db();
		if($db) {
			$select_string = "SELECT levels_t.level AS Level, levels_t.is_time AS IsTime FROM levels_t ORDER BY levels_t.ordering ASC";
			// make query
			$result = @mysqli_query($db, $select_string);
			// succeeded
			if($result) {
				// count rows
				$num_rows = @mysqli_num_rows($result);
				// it better be more than 0
				if($num_rows > 0) {
					for($i=0; $i < $num_rows; $i++) {
						$row = @mysqli_fetch_assoc($result);
						$sort = "DESC";
						if($row['IsTime'] != 0) $sort = "ASC";
						$select_string = "SELECT achievements_t.name AS Name, achievements_t.score AS Score FROM achievements_t WHERE achievements_t.level='".$row['Level']."' ORDER BY achievements_t.score ".$sort." LIMIT 0,1";

						$result2 = @mysqli_query($db, $select_string);
						// succeeded
						if($result2) {
							$num_rows2 = @mysqli_num_rows($result2);
							if($num_rows2 > 0) {
								// values

								// a bit slower but much easier to read
								$row2 = @mysqli_fetch_assoc($result2);

								$place = 1;
								echo "\"".$row['Level']."\" ".$place." \"".$row2['Name']."\" ".$row2['Score']."\n";
								$name2 = $row2['Name'];
								$score2 = $row2['Score'];
							}
						}
						
						if($name!="") {
							$select_string = "SELECT achievements_t.name AS Name, achievements_t.score AS Score FROM achievements_t WHERE achievements_t.level='".$row['Level']."' AND achievements_t.name='".$name."' LIMIT 0,1";

							$result2 = @mysqli_query($db, $select_string);
							// succeeded
							if($result2) {
								$num_rows2 = @mysqli_num_rows($result2);
								if($num_rows2 > 0) {
									// values
									// a bit slower but much easier to read
									$row2 = @mysqli_fetch_assoc($result2);

									$place = 0;
									if($name2 != $row2['Name'] || $score2 != $row2['Score']) {
										//avoid duplicates
										echo "\"".$row['Level']."\" ".$place." \"".$row2['Name']."\" ".$row2['Score']."\n";
									}
								}
							}
						}

					}
				}
			}

			mysqli_close($db);
		}
	}

?>

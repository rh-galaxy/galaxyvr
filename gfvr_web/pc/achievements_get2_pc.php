<?php
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

	require_once("db_connect_pc.php");
	require_once("checkparams.php");

	$steam = 0;
	$user = $_GET["User"];
	$userid = $_GET["UserId"];
	$steam_one = $_GET["IsSteam"];
	$paramArray = array($user,$userid,$steam_one);
	$isok = 0; //no qualify

	if(checkstring($paramArray) && $user!="" && $userid!="") {
		if($steam_one==1) $steam=1;
		
		$db = connect_to_db_pc();

		$query = "SELECT * FROM members_t WHERE oculus_id='".$userid."'";
		$result_member = mysqli_query($db, $query);

		$num_rows = @mysqli_num_rows($result_member);
		if($num_rows==1) {
			$row = @mysqli_fetch_assoc($result_member);
			if($row['username'] == $user) {
				//user and id match
				$query = "UPDATE members_t SET last_access=".time()." WHERE (oculus_id='".$userid."')";
			} else {
				//update user name in members_t since it has changed
				$query = "UPDATE members_t SET username='".$user."',last_access=".time()." WHERE (oculus_id='".$userid."')";
			}
		} else if($num_rows==0) {
			//insert new row in members_t
			$query = "INSERT INTO members_t (username, oculus_id, last_access) VALUES('".$user."','".$userid."',".time().")";
		}
		$result = mysqli_query($db, $query);
		
		if($result) {
			//user id and name now exist in mebers_t, with last_access=NOW()
			$select_string = "SELECT levels_t.level AS Level, levels_t.is_time AS IsTime, levels_t.creator AS Creator, levels_t.limit1,levels_t.limit2,levels_t.limit3 FROM levels_t ORDER BY levels_t.ordering ASC";
			// make query
			$result = @mysqli_query($db, $select_string);
			// succeeded
			if($result) {
				// count rows
				$num_rows = @mysqli_num_rows($result);
				// it better be more than 0
				if($num_rows > 0) {
					for($i=0; $i < $num_rows; $i++) {
////////////////////////////////////////////////////////////////////////////////////////////
//part 1 (per level) limits and player score
						$score = -1;
						$row = @mysqli_fetch_assoc($result);
						$select_string = "SELECT achievements_t.score AS Score FROM achievements_t WHERE achievements_t.user_id='".$userid."' AND achievements_t.level='".$row['Level']."'";

						$result2 = @mysqli_query($db, $select_string);
						// succeeded
						if($result2) {
							$num_rows2 = @mysqli_num_rows($result2);
							if($num_rows2 == 1) {
								// values
								$row2 = @mysqli_fetch_assoc($result2);
								$score = $row2['Score'];
							}
						}
						echo "\"".$row['Level']."\" ".$row['IsTime']." ".$row['limit1']." ".$row['limit2']." ".$row['limit3']." ".$score." ";
						
////////////////////////////////////////////////////////////////////////////////////////////
//part 2 (per level) record scores 1st-3rd place
						$sort = "DESC";
						if($row['IsTime'] != 0) $sort = "ASC";
						$select_string = "SELECT members_t.oculus_id AS Id, achievements_t.score AS Score, members_t.username AS Name FROM achievements_t, members_t WHERE members_t.oculus_id=achievements_t.user_id AND achievements_t.level='".$row['Level']."' ORDER BY achievements_t.score ".$sort." LIMIT 0,3";
						
						$result2 = @mysqli_query($db, $select_string);

						if($result2) {
							$num_rows2 = @mysqli_num_rows($result2);
							$id1 = "0";
							$id2 = "0";
							$id3 = "0";
							for($j=0; $j < 3; $j++) {
								if($j>=$num_rows2) {
									echo "\"_None\" ";
									echo "-1 ";
								} else {
									// a bit slower but easier to read
									$row2 = @mysqli_fetch_assoc($result2);

									//name, score, id
									echo "\"".$row2['Name']."\" ";
									echo $row2['Score']." ";
									if($j==0) $id1 = $row2['Id'];
									if($j==1) $id2 = $row2['Id'];
									if($j==2) $id3 = $row2['Id'];
								}
							}
							echo $id1." ".$id2." ".$id3." "; //patch in id last for version compatibility
						}
						echo "\"".$row['Creator']."\" ";
						
////////////////////////////////////////////////////////////////////////////////////////////
//part 3 (per level) total number of scores
						$select_string = "SELECT count(*) AS Count FROM achievements_t WHERE achievements_t.level='".$row['Level']."'";
						
						$total = -1;
						$result3 = @mysqli_query($db, $select_string);
						if($result3) {
							$num_rows3 = @mysqli_num_rows($result3);
							if($num_rows3==1) {
								$row3 = @mysqli_fetch_assoc($result3);
								$total = $row3['Count'];
							}
						}
						echo $total." ";
						
////////////////////////////////////////////////////////////////////////////////////////////
//part 4 (per level) your place
						$sort = ">";
						if($row['IsTime'] != 0) $sort = "<";
						$select_string = "SELECT count(*) AS Place FROM achievements_t WHERE achievements_t.level='".$row['Level']."' AND achievements_t.score".$sort.$score;
						
						$place = -1;
						if($score!=-1) {
							$result4 = @mysqli_query($db, $select_string);
							if($result4) {
								$num_rows4 = @mysqli_num_rows($result4);
								if($num_rows4==1) {
									$row4 = @mysqli_fetch_assoc($result4);
									$place = $row4['Place'] +1;
								}
							}
						}
						echo $place." ";

////////////////////////////////////////////////////////////////////////////////////////////

						echo "\n";

////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////
//mission levels result row 2 (achievements2_t)
//and race levels result row 2 (achievements2_t)
						{
////////////////////////////////////////////////////////////////////////////////////////////
//part 1 (per level) limits and player score
						$score = -1;
						$select_string = "SELECT achievements2_t.score AS Score FROM achievements2_t WHERE achievements2_t.user_id='".$userid."' AND achievements2_t.level='".$row['Level']."'";

						$result2 = @mysqli_query($db, $select_string);
						// succeeded
						if($result2) {
							$num_rows2 = @mysqli_num_rows($result2);
							if($num_rows2 == 1) {
								// values
								$row2 = @mysqli_fetch_assoc($result2);
								$score = $row2['Score'];
							}
						}
						//decrease limits with 30000 for this mode
						if($row['IsTime'] == 0) echo "\"".$row['Level']."\" ".$row['IsTime']." ".($row['limit1']-30000)." ".($row['limit2']-30000)." ".($row['limit3']-30000)." ".$score." ";
						else                    echo "\"".$row['Level']."\" ".$row['IsTime']." ".($row['limit1']+30000)." ".($row['limit2']+30000)." ".($row['limit3']+30000)." ".$score." ";
						
////////////////////////////////////////////////////////////////////////////////////////////
//part 2 (per level) record scores 1st-3rd place
						$sort = "DESC";
						if($row['IsTime'] != 0) $sort = "ASC";
						$select_string = "SELECT members_t.oculus_id AS Id, achievements2_t.score AS Score, members_t.username AS Name FROM achievements2_t, members_t WHERE members_t.oculus_id=achievements2_t.user_id AND achievements2_t.level='".$row['Level']."' ORDER BY achievements2_t.score ".$sort." LIMIT 0,3";
						
						$result2 = @mysqli_query($db, $select_string);

						if($result2) {
							$num_rows2 = @mysqli_num_rows($result2);
							$id1 = "0";
							$id2 = "0";
							$id3 = "0";
							for($j=0; $j < 3; $j++) {
								if($j>=$num_rows2) {
									echo "\"_None\" ";
									echo "-1 ";
								} else {
									// a bit slower but easier to read
									$row2 = @mysqli_fetch_assoc($result2);

									//name, score, id
									echo "\"".$row2['Name']."\" ";
									echo $row2['Score']." ";
									if($j==0) $id1 = $row2['Id'];
									if($j==1) $id2 = $row2['Id'];
									if($j==2) $id3 = $row2['Id'];
								}
							}
							echo $id1." ".$id2." ".$id3." "; //patch in id last for version compatibility
						}
						echo "\"".$row['Creator']."\" ";
						
////////////////////////////////////////////////////////////////////////////////////////////
//part 3 (per level) total number of scores
						$select_string = "SELECT count(*) AS Count FROM achievements2_t WHERE achievements2_t.level='".$row['Level']."'";
						
						$total = -1;
						$result3 = @mysqli_query($db, $select_string);
						if($result3) {
							$num_rows3 = @mysqli_num_rows($result3);
							if($num_rows3==1) {
								$row3 = @mysqli_fetch_assoc($result3);
								$total = $row3['Count'];
							}
						}
						echo $total." ";
						
////////////////////////////////////////////////////////////////////////////////////////////
//part 4 (per level) your place
						$sort = ">";
						if($row['IsTime'] != 0) $sort = "<";
						$select_string = "SELECT count(*) AS Place FROM achievements2_t WHERE achievements2_t.level='".$row['Level']."' AND achievements2_t.score".$sort.$score;
						
						$place = -1;
						if($score!=-1) {
							$result4 = @mysqli_query($db, $select_string);
							if($result4) {
								$num_rows4 = @mysqli_num_rows($result4);
								if($num_rows4==1) {
									$row4 = @mysqli_fetch_assoc($result4);
									$place = $row4['Place'] +1;
								}
							}
						}
						echo $place." ";

////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////
						echo "\n";
						}
////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////

					}
				}
			}

		}
		mysqli_close($db);
	}

?>

<?php
	//update statistics
	require_once("db_connect_pc.php");
	$db = connect_to_db_pc();
	$query = "UPDATE statistics_t SET count=count+1 WHERE page='achievements_get.php'";
	$info = mysqli_query($db, $query);
	if($db) mysqli_close($db);
?>

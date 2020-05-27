<?php

	require_once("db_connect.php");
	require_once("checkparams.php");

	$db = connect_to_db();

	$query = "SELECT * FROM mpgames_t WHERE last_access>=".(time()-60);
	$result = mysqli_query($db, $query);


	if($result) {
		// count rows
		$num_rows = @mysqli_num_rows($result);
		// it better be more than 0
		if($num_rows > 0) {
			for($i=0; $i < $num_rows; $i++) {
				$row = @mysqli_fetch_assoc($result);
				echo "\"".$row['username']."\" \"".$row['ip']."\"\n";
			}
		} else {
			echo "0\n";
		}
	}
	mysqli_close($db);

?>

<?php
	//update statistics
	require_once("db_connect.php");
	$db = connect_to_db();
	$query = "UPDATE statistics_t SET count=count+1 WHERE page='mpgames_join.php'";
	$info = mysqli_query($db, $query);
	if($db) mysqli_close($db);
?>

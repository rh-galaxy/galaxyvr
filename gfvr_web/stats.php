<?php
	require_once("db_connect.php");
	$db = connect_to_db();
	if($db) {
		$query = "SELECT * FROM statistics_t";
		$info = mysqli_query($db, $query);
		$num_rows = @mysqli_num_rows($info);
		
		for($i=0; $i < $num_rows; $i++) {
			$row = @mysqli_fetch_row($info);
			echo $row[1]." ".$row[2]."\r\n";
		}
		mysqli_close($db);
	}
?>

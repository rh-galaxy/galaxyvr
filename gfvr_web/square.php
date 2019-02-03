<?php

function square_start($width) //example $width = "'100%'";
{
	echo "<table class=square width=".$width."><tr>
	<td class=\"bgcorner1\"></td>
	<td class=\"bgmid1\"></td>
	<td class=\"bgcorner2\"></td>
	</tr><tr>
	<td class=\"bgmid2\"></td>
	<td>";
}

function square_end()
{
	echo "</td>
	<td class=\"bgmid3\"></td>
	</tr><tr>
	<td class=\"bgcorner3\"></td>
	<td class=\"bgmid4\"></td>
	<td class=\"bgcorner4\"></td>
	</tr></table>";
}

?>

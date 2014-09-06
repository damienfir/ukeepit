<!DOCTYPE html>

<html lang="en">
<head>
	<meta charset="utf-8">
	<meta name="viewport" content="width=device-width" />
	<link rel="stylesheet" type="text/css" href="/style.css" />
	<title>ukeep.it</title>
	<meta name="description" content="" />
	<meta name="keywords" content="" />
</head>
<body>

<div style="position:relative; width:200px; margin:100px auto 20px auto; padding:10px 0; color:#444444; font-weight:200; font-size:40px; text-align:center; border-radius:5px">
	ukeep!t
</div>

<div style="width:640px; margin:0 auto; border:2px solid #eeeeee">
	<iframe style="vertical-align:middle" width="640" height="360" src="//www.youtube.com/embed/nPbg-fROphg" frameborder="0" allowfullscreen></iframe>
</div>

<div style="width:640px; margin:40px auto 100px auto">
	<div style="font-weight:200; font-size:20px; margin-bottom:20px; text-align:center">Leave us your e-mail address<br>and we will keep you posted on the upcoming uKeepIt release.</div>
	
	<table style="width:400px; border-spacing:0; margin:0 auto">
		<tr>
			<td style="position:relative; width:338px; padding:5px 5px; border:1px solid #eeeeee; border-bottom-left-radius:5px; border-top-left-radius:5px; background-color:#eeeeee; vertical-align:middle">
				<div id="emailLabel" style="color:#999999; font-size:20px">Your email</div>
				<input style="position:absolute; left:0; top:0; width:100%; padding:5px 5px; background:none; border:none; font-size:20px; vertical-align:middle" type="text" id="email" onkeyup="emailChanged()">
			</td>
			<td style="width:50px; background-color:#444444; border-bottom-right-radius:5px; border-top-right-radius:5px; color:white; font-size:20px; text-align:center; vertical-align:middle; cursor:pointer" onclick="submit()">✓</td>
		</tr>
	</table>
	<div style="padding:10px 0 0 0; text-align:center" id="statusLabel"></div>
</div>

<div style="text-align:center">
	<a style="font-size:12px; color:#444444" href="/">&copy; 2014 di55erent, Lausanne <img src="/SwissFlagRed.png" alt="Swiss flag" style="vertical-align:baseline; position:relative; left:2px; top:1px"></a>
</div>

<script>
var submitButton = document.getElementById('submitButton');
var statusLabel = document.getElementById('statusLabel');
emailChanged();

function emailChanged() {
	var email = document.getElementById('email').value;
	document.getElementById('emailLabel').style.visibility = email == '' ? 'visible' : 'hidden';
	statusLabel.innerHTML = '';
}

function submit() {
	setStatusLabel('Submitting your email address...', '#0000cc');
	var request = new XMLHttpRequest();

	request.onload = function(e) {
		setStatusLabel('We have registered ' + email + '. Thank you!', '#71c837');
		document.getElementById('email').value = '';
		document.getElementById('emailLabel').style.visibility = 'visible';
		//setStatusLabel('Error saving your address!', '#ff0000');
		//window.setTimeout(submit, 10000);
	};

	request.onerror = function(e) {
		request.onload(e);
	};

	var email = document.getElementById('email').value;
	request.open('POST', '/newsletter/' + email, true);
	request.setRequestHeader('Content-Type', 'text/plain');
	request.send('');

	function setStatusLabel(text, color) {
		statusLabel.innerHTML = text;
		statusLabel.style.color = color;
	}
}
</script>

</body>
</html>

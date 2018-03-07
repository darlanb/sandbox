const connection = new signalR.HubConnection('/notification');

connection.on('SendNotification', (message) => {
	const encodedMsg = message;
	var commsLog = document.getElementById("commsLog");
	commsLog.innerHTML += '<tr>' +
		'<td class="commslog-server">Server</td>' +
		'<td class="commslog-client">Client</td>' +
		'<td class="commslog-data">' + htmlEscape(encodedMsg) + '</td>'
	'</tr>';
});

document.getElementById('sendButton').addEventListener('click', event => {
	const msg = document.getElementById('sendMessage').value;
	var commsLog = document.getElementById("commsLog");

	commsLog.innerHTML += '<tr>' +
		'<td class="commslog-client">Client</td>' +
		'<td class="commslog-server">Server</td>' +
		'<td class="commslog-data">' + htmlEscape(msg) + '</td>'
	'</tr>';

	connection.invoke('Subscribe', msg).catch(err => showErr(err));
	event.preventDefault();
});

document.getElementById('connectButton').addEventListener('click', event => {
	connection.start().catch(err => showErr(err));
	var commsLog = document.getElementById("commsLog");
	commsLog.innerHTML += '<tr>' +
		'<td colspan="3" class="commslog-data">Connection opened</td>' +
		'</tr>';
});

document.getElementById('closeButton').addEventListener('click', event => {
	connection.close();
});

function showErr(msg) {
	const listItem = document.createElement('li');
	listItem.setAttribute("style", "color: red");
	listItem.innerText = msg.toString();
	document.getElementById('messages').appendChild(listItem);
}

function htmlEscape(str) {
	return str
		.replace(/&/g, '&amp;')
		.replace(/"/g, '&quot;')
		.replace(/'/g, '&#39;')
		.replace(/</g, '&lt;')
		.replace(/>/g, '&gt;');
}
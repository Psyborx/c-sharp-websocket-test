var ws;

function appendMessage(message) {
	$('#output').append(message + '<br/>');
}

function connectSocketServer() {
	var support = 'WebSocket' in window ? 'WebSocket' : null;

	if (support === null) {
		appendMessage('* Your browser doesn\'t support WebSockets!');
		return;
	}

	appendMessage('* Connecting to server ..');
	// create a new websocket and connect
	ws = new ReconnectingWebSocket('ws://localhost:2012/', null, {debug: true, reconnectInterval: 3000});

	// when data is comming from the server, this metod is called
	ws.onmessage = function (evt) {
		appendMessage('# ' + evt.data);
	};

	// when the connection is established, this method is called
	ws.onopen = function () {
		appendMessage('* Connection open');
		$('#messageInput').prop('disabled', false);
		$('#sendButton').prop('disabled', false);
		$('#connectButton').prop('disabled', true);
		$('#disconnectButton').prop('disabled', false);
	};

	// when the connection is closed, this method is called
	ws.onclose = function () {
		appendMessage('* Connection closed');
		$('#messageInput').prop('disabled', true);
		$('#sendButton').prop('disabled', true);
		$('#connectButton').prop('disabled', false);
		$('#disconnectButton').prop('disabled', true);
	}
}

function sendMessage() {
	if (ws) {
		var messageBox = $('#messageInput');
		ws.send(messageBox.val());
		messageBox.val('');
	}
}

function disconnectWebSocket() {
	if (ws) {
		ws.close(1000, 'Disconnection');
	}
}

function connectWebSocket() {
	connectSocketServer();
}

function clearOutput() {
	$('#output').html('');
}

$(document).ready(function() {
	$('#connectButton').click(connectWebSocket);
	$('#disconnectButton').click(disconnectWebSocket);
	$('#sendButton').click(sendMessage);
	$('#clearButton').click(clearOutput);
});


function methodFormSubmit() {
    if ($('#methodRadioEmail').prop('checked')) {
        $('#formcontainer').load('/Signature/EMailMethod');
    }
    else if ($('#methodRadioSMS').prop('checked')) {
        $('#formcontainer').load('/Signature/SMSMethod');
    }
    else if ($('#methodRadioLogin').prop('checked')) {
        $('#formcontainer').load('/Signature/LoginMethod');
    }
    else if ($('#methodRadioWallet').prop('checked')) {
        $('#formcontainer').load('/Signature/WalletMethod');
    }
    else if ($('#methodRadioCertificate').prop('checked')) {
        $('#formcontainer').load('/Signature/CertificateMethod');
    }
}

function emailFormSubmit(url) {
    var data = {
        email: $('#emailInput').val(),
        name: $('#nameInput').val()
    };

    $.post(url, data, function (data) {
        $('#formcontainer').html(data);
    });
}
function loginFormSubmit(url) {
    var data = {
        email: $('#emailInput').val(),
        password: $('#passwordInput').val()
    };

    $.post(url, data, function (data) {
        $('#formcontainer').html(data);
    });
}

function confirmationCodeSubmit(url) {
    var data = {
        confirmationcode: $('#confirmationcode').val()
    };

    $.post(url, data, function (data) {
        $('#formcontainer').html(data);
    });
}


function certificateFormSubmit(url) {
    var data = {
        email: $('#emailInput').val(),
        name: $('#nameInput').val()
    };

    $.post(url, data, function (data) {
        $('#formcontainer').html(data);
    });
}

function initHandsignInput(obj) {
    var handsign = '';
    var input = $('#handsign');
    var canvas = document.getElementById('handsignCanvas');
    var ctx = canvas.getContext("2d");
    var w = $(canvas).parent().width();
    var h = w / 2;

    canvas.width = w;
    canvas.height = h;

    ctx.fillStyle = '#dddddd';
    ctx.fillRect(0, 0, w, h);
    ctx.strokeStyle = '#c3124c';
    ctx.lineWidth = 2;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';

    var lastX = 0;
    var lastY = 0;
    var isDown = false;

    canvas.addEventListener("mousemove", function (e) {
        if (!isDown)
            return;
        var drawX = e.clientX - canvas.getBoundingClientRect().left;
        var drawY = e.clientY - canvas.getBoundingClientRect().top;
        var x = Math.round(drawX * 400 / w);
        var y = Math.round(drawY * 200 / h);

        if (x != lastX || y != lastY) {
            ctx.lineTo(drawX, drawY);
            ctx.stroke();
            lastX = x;
            lastY = y;
            handsign += "," + x.toString() + ":" + y.toString();
            input.val(handsign);
            $('#signcode').html(handsign);
        }

    }, false);
    canvas.addEventListener("mousedown", function (e) {
        var drawX = e.clientX - canvas.getBoundingClientRect().left;
        var drawY = e.clientY - canvas.getBoundingClientRect().top;
        var x = Math.round(drawX * 400 / w);
        var y = Math.round(drawY * 200 / h);
        ctx.beginPath();
        ctx.moveTo(drawX, drawY);
        isDown = true;
        lastX = x;
        lastY = y;
        if (handsign.length == 0)
            handsign += x.toString() + ":" + y.toString();
        else
            handsign += ";" + x.toString() + ":" + y.toString();
        input.val(handsign);
        $('#signcode').html(handsign);
    }, false);
    canvas.addEventListener("mouseup", function (e) {
        if (!isDown)
            return;
        var drawX = e.clientX - canvas.getBoundingClientRect().left;
        var drawY = e.clientY - canvas.getBoundingClientRect().top;
        var x = Math.round(drawX * 400 / w);
        var y = Math.round(drawY * 200 / h);

        //ctx.lineTo(drawX, drawY);
        //ctx.stroke();
        //handsign += "," + x.toString() + ":" + y.toString();
        //input.val(handsign);
        //$('#signcode').html(handsign);
        
        ctx.closePath();
        isDown = false;
        lastX = 0;
        lastY = 0;
    }, false);
    canvas.addEventListener("mouseout", function (e) {
        if (!isDown)
            return;
        var drawX = e.clientX - canvas.getBoundingClientRect().left;
        var drawY = e.clientY - canvas.getBoundingClientRect().top;
        var x = Math.round(drawX * 400 / w);
        var y = Math.round(drawY * 200 / h);

        ctx.lineTo(drawX, drawY);
        ctx.stroke();
        handsign += "," + x.toString() + ":" + y.toString();
        input.val(handsign);
        $('#signcode').html(handsign);
        
        ctx.closePath();
        isDown = false;
        lastX = 0;
        lastY = 0;
    }, false);

    // Set up touch events for mobile, etc
    canvas.addEventListener("touchstart", function (e) {
        mousePos = getTouchPos(canvas, e);
        var touch = e.touches[0];
        var mouseEvent = new MouseEvent("mousedown", {
            clientX: touch.clientX,
            clientY: touch.clientY
        });
        canvas.dispatchEvent(mouseEvent);
    }, false);
    canvas.addEventListener("touchend", function (e) {
        mousePos = getTouchPos(canvas, e);
        var touch = e.touches[0];
        var mouseEvent = new MouseEvent("mouseup", {
            clientX: touch.clientX,
            clientY: touch.clientY
        });
        canvas.dispatchEvent(mouseEvent);
    }, false);
    canvas.addEventListener("touchmove", function (e) {
        var touch = e.touches[0];
        var mouseEvent = new MouseEvent("mousemove", {
            clientX: touch.clientX,
            clientY: touch.clientY
        });
        canvas.dispatchEvent(mouseEvent);
    }, false);

    // Get the position of a touch relative to the canvas
    function getTouchPos(canvasDom, touchEvent) {
        var rect = canvasDom.getBoundingClientRect();
        return {
            x: touchEvent.touches[0].clientX - rect.left,
            y: touchEvent.touches[0].clientY - rect.top
        };
    }

    document.body.addEventListener("touchstart", function (e) {
        if (e.target == canvas) {
            e.preventDefault();
        }
    }, false);
    document.body.addEventListener("touchend", function (e) {
        if (e.target == canvas) {
            e.preventDefault();
        }
    }, false);
    document.body.addEventListener("touchmove", function (e) {
        if (e.target == canvas) {
            e.preventDefault();
        }
    }, false);
}

function renderHandsign(canvasID, data) {
    var canvas = document.getElementById(canvasID);
    var ctx = canvas.getContext("2d");
    var w = Math.min(400, $(canvas).parent().width());
    var h = w / 2;

    canvas.width = w;
    canvas.height = h;

    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, w, h);
    ctx.strokeStyle = '#c3124c';
    ctx.lineWidth = 2;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';

    var paths = data.split(';')
    for (var p = 0; p < paths.length; p++) {
        var points = paths[p].split(',');

        ctx.beginPath();

        for (var i = 0; i < points.length; i++) {
            var coords = points[i].split(':');
            var x = parseFloat(coords[0]) * w / 400;
            var y = parseFloat(coords[1]) * h / 200;

            if (i == 0) {
                ctx.moveTo(x, y);
            }
            else {
                ctx.lineTo(x, y);
                ctx.stroke();
            }
        }

        ctx.closePath();
    }
}
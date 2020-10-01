$(document).ready(function () {

$(function () {
    var idselected;
    var vert = 0;
	// Create the QuaggaJS config object for the live stream
	var liveStreamConfig = {
            inputStream: {
            type : "LiveStream",
				constraints: {
                    width: { min: 1280},
                    height: { min: 720 },
					aspectRatio: {min: 1, max: 100},
					facingMode: "environment" // or "user" for the front camera
                },
                area: { // defines rectangle of the detection/localization area
                    top: "39%",    // top offset
                    right: "12%",  // right offset
                    left: "12%",   // left offset
                    bottom: "39%"  // bottom offset
                }
			},
			locator: {
            patchSize: "large",
                halfSample: false
                //debug: {
                //    showCanvas: true,
                //    showPatches: false,
                //    showFoundPatches: false,
                //    showSkeleton: false,
                //    showLabels: false,
                //    showPatchLabels: false,
                //    showRemainingPatchLabels: false,
                //    boxFromPatches: {
                //        showTransformed: false,
                //        showTransformedBox: false,
                //        showBB: false
                //    }
                //}
        },
        tracking: false,
        debug: false,
        controls: false,
        numOfWorkers: (navigator.hardwareConcurrency ? navigator.hardwareConcurrency : 4),
        decoder: {
            drawBoundingBox: true,
            showFrequency: true,
            drawScanline: true,
            showPattern: true,
            readers: [
                { "format": "code_128_reader","config":{}}
                ]
			},
        locate: false,

    };

    var liveStreamConfigVertical = {
        inputStream: {
            type: "LiveStream",
            constraints: {
                width: { min: 1280 },
                height: { min: 720 },
                aspectRatio: { min: 1, max: 100 },
                facingMode: "environment" // or "user" for the front camera
            },
            area: { // defines rectangle of the detection/localization area
                top: "0%",    // top offset
                right: "0%",  // right offset
                left: "0%",   // left offset
                bottom: "0%"  // bottom offset
            }
        },
        locator: {
            patchSize: "large",
            halfSample: false
            //debug: {
            //    showCanvas: true,
            //    showPatches: false,
            //    showFoundPatches: false,
            //    showSkeleton: false,
            //    showLabels: false,
            //    showPatchLabels: false,
            //    showRemainingPatchLabels: false,
            //    boxFromPatches: {
            //        showTransformed: false,
            //        showTransformedBox: false,
            //        showBB: false
            //    }
            //}
        },
        tracking: false,
        debug: false,
        controls: false,
        numOfWorkers: (navigator.hardwareConcurrency ? navigator.hardwareConcurrency : 4),
        decoder: {
            drawBoundingBox: true,
            showFrequency: true,
            drawScanline: true,
            showPattern: true,
            readers: [
                { "format": "code_128_reader", "config": {} }
            ]
        },
        locate: true,

    };

    var finalConfig;


	// The fallback to the file API requires a different inputStream option.
	// The rest is the same
	var fileConfig = $.extend(
			{},
			finalConfig,
			{
            inputStream: {
            size: 800
				}
			}
		);
	// Start the live stream scanner when the modal opens
    $('#livestream_scanner').on('shown.bs.modal', function (e) {
        //var opener = document.activeElement;

        if (vert == 1) {
            finalConfig = liveStreamConfigVertical;
        } else {
            finalConfig = liveStreamConfig;
        }

            Quagga.init(
                finalConfig,
                function (err) {
                    if (err) {
                        $('#livestream_scanner .modal-body .error').html('<div class="alert alert-danger"><strong><i class="fa fa-exclamation-triangle"></i> ' + err.name + '</strong>: ' + err.message + '</div>');
                        Quagga.stop();
                        return;
                    }
                    Quagga.start();
                }
        );

        var opener = $(e.relatedTarget);
        if (vert == 0) {
            idselected = $(opener).attr('id');
            console.log(idselected);
        }



        });

	// Make sure, QuaggaJS draws frames an lines around possible
	// barcodes on the live stream
	Quagga.onProcessed(function(result) {
        var drawingCtx = Quagga.canvas.ctx.overlay;
			//drawingCanvas = Quagga.canvas.dom.overlay;
        var drawingCanvas = Quagga.canvas.dom.overlay;
        //drawingCanvas.style.display = 'block';

		if (result) {
			if (result.boxes) {

                drawingCtx.clearRect(0, 0, parseInt(drawingCanvas.getAttribute("width")), parseInt(drawingCanvas.getAttribute("height")));

                result.boxes.filter(function (box) {
                    Quagga.ImageDebug.drawPath(box, { x: 0, y: 1 }, drawingCtx, { color: "green", lineWidth: 2 });
                    return box !== result.box;
                });
			}

			if (result.box) {
            Quagga.ImageDebug.drawPath(result.box, { x: 0, y: 1 }, drawingCtx, { color: "#00F", lineWidth: 2 });
        }

			if (result.codeResult && result.codeResult.code) {
            Quagga.ImageDebug.drawPath(result.line, { x: 'x', y: 'y' }, drawingCtx, { color: 'red', lineWidth: 3 });
        }
		}
	});

	// Once a barcode had been read successfully, stop quagga and
	// close the modal after a second to let the user notice where
	// the barcode had actually been found.
    Quagga.onDetected(function (result) { 

        if (result.codeResult.code) {
           
            //$('#scanner_input').val(result.codeResult.code);
            var res = "#scanner_input_" + idselected;

           $('input' + res + '').val(result.codeResult.code);
           var codeFreezer = result.codeResult.code;
            var comments =$('#commentsreceive').val();

            Quagga.stop();

            setTimeout(function () { $('#livestream_scanner').modal('hide'); }, 1000);
            vert = 0;


                    $.ajax({
                        url: '/Invoices/receiveAssignedFreezer',
                        type: 'POST',
                        data: { 'code': codeFreezer, 'comments': comments },
                        cache: false,
                        global: false,
                        success: function (result) {

                            if (result == "success") {
                                $('input' + res + '').val("");
                                $('#commentsreceive').val("")
                                Swal.fire(
                                    'Freezer received!',
                                    'The Freezer has been received successfully.',
                                    'success'
                                )



                            } else {
                                toastr.info(result);
                                $('input' + res + '').val("");
                       
                            }


                        },
                        error: function (request) {
                            toastr.warning("Something went wrong..");
                            $('input' + res + '').val("");
               

                        }
                    });




        }
	});

	// Stop quagga in any case, when the modal is closed
    $('#livestream_scanner').on('hide.bs.modal', function(){
    	if (Quagga){
            Quagga.stop();

        }
    });

	// Call Quagga.decodeSingle() for every file selected in the
	// file input
	$("#livestream_scanner input:file").on("change", function(e) {
		if (e.target.files && e.target.files.length) {
            Quagga.decodeSingle($.extend({}, fileConfig, { src: URL.createObjectURL(e.target.files[0]) }), function (result) { alert(result.codeResult.code); });
            
            
        }
	});

    $('#vertbtn').on('click', function () {
        setTimeout(function () { $('#livestream_scanner').modal('hide') }, 10)
        setTimeout(function () { $('#livestream_scanner').modal('show') }, 900)
        vert = 1;
        document.getElementById('vertbtn').style.display = "none";
    });


    $('#closebtn').on('click', function () {
        setTimeout(function () { $('#livestream_scanner').modal('hide') }, 10)
        vert = 0;
        document.getElementById('vertbtn').style.display = "block";
    });

});
});
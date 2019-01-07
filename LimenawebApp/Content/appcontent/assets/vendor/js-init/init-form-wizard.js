$(function() {
    $('#default').stepy({
        backLabel: 'Previous',
        block: true,
        nextLabel: 'Next',
        titleClick: true,
        validate: true,
        titleTarget: '.stepy-tab',
        next: function (index) {
            if (index == 2) {
                var selCustomer = $("#ID_customer").val();
                var selCName = $("#ID_customer option:selected").text();
                
                var selPayment = $("#ID_payment").val();
                var selPName = $("#ID_payment option:selected").text();

                var docnum = $("#doc_number").val();
                var company = $("#doc_numberComp").val();
  
                var flag = 0;

                if (selCustomer == 0) {
                    alert("Select a customer");   
                    flag = 1;
                    
                }



                if (flag == 0) {
                    //$("#doc_number").css("border-color", "#ced4da");
                    //$("#doc_numberComp").css("border-color", "#ced4da");

                } else {
                    return false;
                }

            }
        }// finish: function () {

        //    alert('Canceling...');

        //    return false;

        //}

    });
});

$(document).ready(function () {
    var form = $("#wizard-validation-form");
    form.validate({
        errorPlacement: function errorPlacement(error, element) {
            element.after(error);
        }
    });
    form.children("div").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        onStepChanging: function (event, currentIndex, newIndex) {
            form.validate().settings.ignore = ":disabled,:hidden";
            return form.valid();
        },
        onFinishing: function (event, currentIndex) {
            form.validate().settings.ignore = ":disabled";
            return form.valid();
        },
        onFinished: function (event, currentIndex) {
            alert("Submitted!");
        }
    }).validate({
        errorPlacement: function errorPlacement(error, element) {
            element.after(error);
        },
        rules: {
            confirm: {
                equalTo: "#password"
            }
        }
    });
});



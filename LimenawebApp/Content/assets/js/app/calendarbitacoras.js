(function ($) {
	"use strict";

  var calendar,
  e,
  noticed = false,
  option = {
    header: {
      left: 'title, prev, next',
      center: '',
      right: ''
      },
      minTime: "04:00:00",
      maxTime: "22:00:00",
    contentHeight: 'auto',
      defaultView: 'agendaDay',

      validRange: {
          start: visibleInit,
          end: visibleEnd
      },
    defaultDate: moment().format('YYYY-MM-DD'),
      editable: true,
      locale: 'es',
    eventLimit: false,
    viewRender: function (view, element) {
      // style
      element.find('th.fc-day-header.fc-widget-header').each(function () {
        if($(this).data('date')){
          var date = moment($(this).data('date'));
          $(this).html('<span>' + date.format('D') + '</span><span class="fc-week-title">' + date.format('dddd') + '</span>');
        }
      })
    },
    eventRender: function(event, element) {
      // render
      element.find('.fc-content').append('<div class="mt-1 text-muted">'+event.description+'</div>');
      //element.find('.fc-content').append( '<div class="d-flex my-3 avatar-group">' + getParticipant(event, 24) + '</div>');
    },
    eventClick: function(event, jsEvent) {
        $('#modal-nuevaActividad').modal('show');
      e = event;
      getEvent(event);
      },
      eventDrop: function (event, delta, revertFunc) {
          var init = new Date(event.start);

          if (init.withoutTime() < originalDate.withoutTime()) {
              revertFunc();
              toastr.info('Rango de tiempo no valido');
          }
         

      },
      eventResize: function (event, delta, revertFunc) {
          var initd = new Date(event.end);
          var endt = new Date(event.start);

          if (initd.withoutTime() > endt.withoutTime()) {
              revertFunc();
              toastr.info('Rango de tiempo no valido');
          }
      },
      eventDragStart: function (event) {
          originalDate = new Date(event.start);  // Make a copy of the event date
      },
        };

    Date.prototype.withoutTime = function () {
        var d = new Date(this);
        d.setHours(0, 0, 0, 0);
        return d;
    }
    var originalDate;
  function setupEvents(){
    $(document).on('click', '#dayview', function() {
      calendar.fullCalendar('changeView', 'agendaDay');
      sr.sync();
    });

    $(document).on('click', '#weekview', function() {
      calendar.fullCalendar('changeView', 'agendaWeek');
      sr.sync();
    });



    $(document).on('click', '#todayview', function() {
      calendar.fullCalendar('today');
      sr.sync();
    });

    $(document).on('click', '#btn-new', function() {
      $('#newEvent').modal('show');
      e = {title:'', description:'', start:moment(), end:moment().add(4, 'h'), participant:'1', type:'', className: 'block b-t b-t-2x b-primary'};
      getEvent(e);
    });

    $(document).on('click', '#btn-save', function() {
      var e = getEvent();
      if(e.id){
        calendar.fullCalendar( 'updateEvent', e );
      }else{
        e.id = moment().toDate();
        calendar.fullCalendar( 'renderEvent', e );
      }
      $('#newEvent').modal('hide');
    });
  }

  function getEvent(event){
    var el_title = $('#event-title'),
        el_desc = $('#event-desc'),
        el_type = $('#event-type'),
        el_start_date = $('#event-start-date'),
        el_start_time = $('#event-start-time'),
        el_end_date = $('#event-end-date'),
        el_end_time = $('#event-end-time'),
        el_participant = $('#event-participant');
    // set date to form
    if(event){
      el_title.text(event.title);
      el_desc.val(event.description);


      //el_participant.html( getParticipant(event, 32) );
        el_start_date.text(moment(event.start).format("YYYY-MM-DD"));
        el_start_time.text(moment(event.start).format("HH:mm"));

        

        //Consultamos datos
        $.ajax
            ({
                url: '/Planificacion/Get_BitacoraDetalles',
                type: 'POST',
                datatype: 'application/json',
                contentType: 'application/json',
                data: JSON.stringify({
                    ID_bitacora: event.id
                }),
                success: function (result) {



                    $("#idbitacorasel").val(event.id);
                    $("#idbitacsel2").val(result.detalleBitacora.ID_actividad);
                    $("#descbitacorasel").val(event.title);
                    $("#event-titleTareas").text(event.title);

                    $("#nueva_prioridad").val(result.detalleBitacora.Prioridad);
                    $("#nueva_fuenteentrada").val(result.detalleBitacora.Fuente_entrada);


                    if (result.detalleBitacora.Form_trabajo == "Presencial") {
                        $("#forma1").prop("checked", true);
                        $("#forma2").prop("checked", false);
                    } else {
                        $("#forma1").prop("checked", false);
                        $("#forma2").prop("checked", true);
                    }





                    $("#tbtareasacct tbody").html("");
                    $.each(result.detalleBitacora.lsttareas, function (i, tareas) {
                        var newRowContent = ""; 
                        if (result.detalleBitacora.Finalizada) {
                            if (tareas.Finalizada) {
                                newRowContent = '<tr><td>' + tareas.ID_tarea + '</td><td>' + tareas.actividad + ' </td><td>' + tareas.Comentarios + ' </td><td><label class="text-success"><i class="material-icons ml-2">check</i></label></td><td></td></tr>';
                            } else {
                                newRowContent = '<tr><td>' + tareas.ID_tarea + '</td><td>' + tareas.actividad + ' </td><td>' + tareas.Comentarios + ' </td><td><label class=" text-success"><i class="material-icons ml-2">check</i></label></td><td></td></tr>';
                            }
                        } else {
                            if (tareas.Finalizada) {
                                newRowContent = '<tr><td>' + tareas.ID_tarea + '</td><td>' + tareas.actividad + ' </td><td>' + tareas.Comentarios + ' </td><td><div class="custom-control custom-checkbox-toggle custom-control-inline mr-1"> <input type="checkbox" checked="" id = "fintarea_' + tareas.ID_tarea + '" class="custom-control-input checkboxesindiv" data-tarea="' + tareas.ID_tarea + '" onchange="countchecksind(this)"><label class="custom-control-label" for="fintarea_' + tareas.ID_tarea + '"></label> </div ></td><td><button type="button" class="btn btn-sm btn-danger" data-tarea="' + tareas.ID_tarea + '" onClick="delTarea(this)">Eliminar</button></td></tr>';
                            } else {
                                newRowContent = '<tr><td>' + tareas.ID_tarea + '</td><td>' + tareas.actividad + ' </td><td>' + tareas.Comentarios + ' </td><td><div class="custom-control custom-checkbox-toggle custom-control-inline mr-1"> <input type = "checkbox" id = "fintarea_' + tareas.ID_tarea + '" class="custom-control-input checkboxesindiv" data-tarea="' + tareas.ID_tarea + '"  onchange="countchecksind(this)"><label class="custom-control-label" for="fintarea_' + tareas.ID_tarea + '"></label> </div ></td><td><button type="button" class="btn btn-sm btn-danger"  data-tarea="' + tareas.ID_tarea + '" onClick="delTarea(this)">Eliminar</button></td></tr>';
                            }
                        }
                 
                        
                    
                    
                        $("#tbtareasacct tbody").append(newRowContent);
                    
                    });

        
                    countchecksindonLOAD();
                    if (result.detalleBitacora.Finalizada) {
                        $("#btnsaveBitacora").hide();
                        $("#nuevahorainicio").prop("disabled", true);
                        $("#nueva_prioridad").prop("disabled", true);
                        $("#nueva_fuenteentrada").prop("disabled", true);
                        $("input[name='formatrabajo']").prop("disabled", true);
                        $("#nuevahorafin").prop("disabled", true);
                        $("#btnsubFinalizada").hide();
                        $("#btnnuevaTarea").hide();
                    } else {
                        $("#btnsaveBitacora").show();
                        $("#nuevahorainicio").prop("disabled", false);
                        $("#nueva_prioridad").prop("disabled", false);
                        $("#nueva_fuenteentrada").prop("disabled", false);
                        $("input[name='formatrabajo']").prop("disabled", false);
                        $("#nuevahorafin").prop("disabled", false);
                        $("#btnsubFinalizada").show();
                        $("#btnnuevaTarea").show();
                    }
                },
                error: function () {
                    alert("Whooaaa! Something went wrong..")
                },
            });


        $("#nuevahorainicio").flatpickr({
            enableTime: true,
            noCalendar: true,
            dateFormat: "H:i",
            time_24hr: false,
            static:true,
            defaultDate: moment(event.start).format("HH:mm")
        });
        //$("#nuevahorainicio").val(moment(event.start).format("HH:mm"));

        $("#nuevahorafin").flatpickr({
            enableTime: true,
            noCalendar: true,
            dateFormat: "H:i",
            time_24hr: false,
            static: true,
            defaultDate: moment(event.end).format("HH:mm")
        });

        el_end_date.text(moment(event.end).format("YYYY-MM-DD"));
        el_end_time.text(moment(event.end).format("HH:mm"));
      
    // get data from form
    }else{
       el_title.text(event.title);

    }
    return e;
  }

  //function getParticipant(event, size){
  //  var participant = '';
  //  var size = size || 24;
  //  $.each(event.participant.split(','), function (index, value) {
  //    participant += '<a href="#" class="avatar w-'+size+'"><img src="../assets/img/a'+value+'.jpg"></a>';
  //  });
  //  return participant;
  //}

    var init = function () {
        var data = eventsCalendar;
 //   var data=[
 //   {
 //     "id": 1,
 //     "type": "Meeting",
 //     "title": "New Project Kick-off meeting",
 //     "description": "Augue onec eleifend nisl eu",
 //     "className": "block b-t b-t-2x b-primary",
 //     "participant": "1,2,4,7,9"
 //   },
 //   {
 //     "id": 2,
 //     "type": "Appointment",
 //     "title": "Meet Mike at company office",
 //     "description": "Phasellus at ultricies neque augue",
 //     "className": "block b-t b-t-2x b-info",
 //     "participant": "4,5,7,8"
 //   },
 //   {
 //     "id": 3,
 //     "type": "Meeting",
 //     "title": "Travel IOS application mockup design",
 //     "description": "Quis malesuada augue",
 //     "className": "block b-t b-t-2x b-success",
 //     "participant": "6,7"
 //   },
 //   {
 //     "id": 4,
 //     "type": "Appointment",
 //     "title": "Weekly report meeting",
 //     "description": "Donec eleifend nisl eu consectetur.",
 //     "className": "block b-t b-t-2x b-danger",
 //     "participant": "3,6"
 //   },
 //   {
 //     "id": 5,
 //     "type": "Meeting",
 //     "title": "All day Meeting",
 //     "description": "Malesuada augue onec eleifend nisl eu.",
 //     "className": "block b-t b-t-2x b-warning",
 //     "participant": "2,6,8,9"
 //   },
 //   {
 //     "id": 6,
 //     "type": "Meeting",
 //     "title": "Repeating Meeting",
 //     "description": "Donec eleifend nisl eu consectetur.",
 //     "className": "block b-t b-t-2x b-primary",
 //     "participant": "6,9"
 //   }
 //];

      // make up the start / end date
      $.each(data, function(index,item){
          item.start = new Date(parseInt(item.start.substr(6)));
          item.end = new Date(parseInt(item.end.substr(6)));

      });
      option.events = data;
      calendar = $('#fullcalendar').fullCalendar(option);

      //sr.reveal('#fullcalendar', {viewFactor: 0, delay: 10, origin: 'left', distance:'100vw', scale: 1});
      //sr.reveal('.fc-event', {
      //  viewFactor: 0, 
      //  delay: 500,
      //  afterReveal:function (el) {
      //    $(el).css('transform', 'none');
      //    if(!noticed){
      //      notie.alert({ text: 'Start on "Add Event" button', position: 'top' });
      //      noticed = true;
      //    }
      //  }
      //}, 50);

      setupEvents();
   
  }

  // for ajax to init again
  $.fn.fullCalendar.init = init;

})(jQuery);

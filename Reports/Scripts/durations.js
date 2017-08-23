function ChartData(opts) {
    /// <summary>Determines the chart scale. Handles convertions of durations to pixel lengths.</summary>
    /// <param name="opts" value="{startDate:'',endDate:'',durations:[{ReservationID:0,ChargeBeginDateTime:'',Parts:[{DurationType:'',Duration:''}]}]}">The total chart duration.</param>
    /// <field name="startDate" value="moment()">The date range start date for the data.</field>
    /// <field name="endDate" value="moment()">The date range end date for the data.</field>
    /// <field name="durations" value="[new DurationInfo()]">A collection of DurationInfo objects.</field>
    
    this.startDate = moment(opts.startDate);
    this.endDate = moment(opts.endDate);

    var diff = this.endDate.diff(this.startDate);

    this.totalDuration = moment.duration(diff);

    var _durations = [];
    
    $.each(opts.durations, function (index, value) {
        _durations.push(new DurationInfo(value));
    });

    this.durations = _durations;
}

function ChartScale(duration, ppu) {
    /// <summary>Determines the chart scale. Handles convertions of durations to pixel lengths.</summary>
    /// <param name="duration" value="moment.duration()">The total chart duration.</param>
    /// <param name="ppu" type="Number">The number of pixels per unit.</param>
    /// <field name="duration" value="moment.duration()">The total chart duration.</field>
    /// <field name="units" type="String">The scale units: minutes, hours, or days.</field>
    /// <field name="value" type="Number">The number of units in the total chart duration.</field>
    /// <field name="increment" type="Number">The number of units between each vertical grid line.</field>
    
    var _self = this;

    var _units, _value, _increment;
    
    var totalMinutes = duration.asMinutes();

    if (totalMinutes > 1440 * 7) {
        // greater than 1 weeks
        _value = duration.asDays();
        _units = "days";
        _increment = 1; // one tick mark for every one day
    } else if (totalMinutes > 1440) {
        // greater than 1 day
        _value = duration.asHours();
        _units = "hours";
        _increment = 1 // one tick mark for every one hour
    } else {
        _value = totalMinutes;
        _units = "minutes";
        _increment = 5; // one tick mark for every five minutes
    }

    this.isMajor = function (time) {
        /// <summary>Determines if the specified time is a major tick mark.</summary>
        /// <param name="time" value="moment()">The tick mark time.</param>
        /// <returns type="Boolean">True when the time is for a major tick mark, otherwise false.</returns>
        
        console.log(time.format("MM/DD/YYYY hh:mm:ss a") + ": " + time.days() + " days, " + time.hours() + " hours, " + time.minutes() + " minutes");

        switch (_units) {
            case "days":
                // major tick mark every 7 days
                return time.days() % 7 == 0;
            case "hours":
                // major tick mark every 4 hours
                return time.hours() % 4 == 0;
            default:
                // major tick mark every 30 minutes
                return time.minutes() % 30 == 0
        }
    };

    this.getLength = function (duration) {
        /// <summary>Finds the number of pixels needed to represent the specified duration.</summary>
        /// <param name="duration" value="moment.duration()">The duration for which to determine the length.</param>
        /// <returns type="Number">The length of duration in pixels.</returns>
        
        switch (_units) {
            case "days":
                return duration.asDays() * 30 * ppu;
            case "hours":
                return duration.asHours() * 15 * ppu;
            default:
                return duration.asMinutes() * ppu;
        }
    }

    this.getTotalLength = function () {
        /// <summary>Finds the number of pixels needed to represent the entire duration.</summary>
        /// <returns type="Number">The length of the entire duration in pixels.</returns>

        return _self.getLength(duration);
    }

    this.duration = moment.duration(_value, _units);
    this.units = _units;
    this.value = _value;
    this.increment = _increment;
}

function DurationInfo(value) {
    /// <summary>Represents a single reservation that is comprised of one or more duration parts.</summary>
    /// <param name="value" value="{ReservationID:0,ChargeBeginDateTime:'',Parts:[{DurationType:'',Duration:''}]}">An object containing initial field values.</param>
    /// <field name="reservationId" type="Number">The unique id for this reservation.</field>
    /// <field name="start" value="moment()">The start time for this reservation.</field>
    /// <field name="parts" value="[new DurationPart()]">The duration components.</field>

    this.reservationId = value.ReservationID;
    this.start = moment(value.ChargeBeginDateTime);

    var _parts = [];

    $.each(value.Parts, function (index, value) {
        _parts.push(new DurationPart(value));
    });

    this.parts = _parts;
}

function DurationPart(value) {
    /// <summary>Represents a single reservation that is comprised of one or more duration parts.</summary>
    /// <param name="value" value="{DurationType:'',Duration:''}">An object containing initial field values.</param>
    /// <field name="durationType" type="String">Specifies the type of duation, for example overtime or standard.</field>
    /// <field name="duration" value="moment.duration()">The duration.</field>

    this.durationType = new String(value.DurationType);
    this.duration = moment.duration(value.Duration);
}

function DurationChart(options) {
    /// <summary>A chart that displays the durations of overlapping reservations.</summary>
    /// <param name="options" value="{startDate:'',endDate:'',durations:[{ReservationID:0,ChargeBeginDateTime:'',Parts:[{DurationType:'',Duration:''}]}],pixelsPerUnit:0,backgroundColor:'',colors:{}}">Options for displaying the chart.</param>
    /// <field name="pixelsPerUnit" type="Number">The number of pixels displayed for each unit (minute, hour, day). The unit used is dynamically based on the total chart duration.</param>
    /// <field name="backgroundColor" type="String">The background color of the chart.</param>
    /// <field name="data" type="ChartData">The data used to create the chart.</param>
    /// <field name="scale" type="ChartScale">The scale uesd to create the chart. Determins if the vertical grid lines represent minutes, hours, or days.</param>
    /// <field name="colors" value="{}">A hash that maps each duration type to a specific color.</field>

    var _self = this;

    _self.pixelsPerUnit = isNaN(options.pixelsPerUnit) ? 4 : parseInt(options.pixelsPerUnit);
    _self.backgroundColor = (options.backgroundColor) ? new String(options.backgroundColor) : null;
    _self.data = new ChartData(options);
    _self.scale = new ChartScale(_self.data.totalDuration, _self.pixelsPerUnit);

    var defaultColors = {
        "T": "rgba(204, 204, 0, 0.5)",
        "S": "rgba(0, 128, 0, 0.5)",
        "O": "rgba(128, 0, 0, 0.5)",
    };
    
    _self.colors = $.extend({}, defaultColors, options.colors);
    
    var scaleTop = 20;
    var scaleLeft = 70.5;

    var drawScale = function (canvas, ctx) {
        /// <summary>Draws the chart scale.</summary>
        /// <param name="canvas" type="HTMLCanvasElement">The canvas on which to draw.</param>
        /// <param name="ctx" type="CanvasRenderingContext2D">The current drawing context.</param>
        
        ctx.fillStyle = "#000000";
        ctx.font = "12px 'Courier New'";

        ctx.textAlign = "right";
        ctx.fillText(_self.data.startDate.format("MM/DD"), scaleLeft - 5.5, scaleTop - 10);
        ctx.fillText(_self.data.startDate.format("hh:mm a"), scaleLeft - 5.5, scaleTop);

        // draw the horizontal line under the time
        var horizontalLineTop = scaleTop + 5.5;
        var verticalLineTop = scaleTop - 20;

        ctx.beginPath();
        ctx.moveTo(0, horizontalLineTop);
        ctx.lineTo(canvas.width, horizontalLineTop);
        ctx.stroke();

        // draw the vertical line at the start of the date range
        ctx.beginPath();
        ctx.moveTo(scaleLeft, verticalLineTop);
        ctx.lineTo(scaleLeft, canvas.height);
        ctx.stroke();

        //4 pixels per minute is default
        var i = _self.scale.increment;

        var prevDay = _self.data.startDate.format("MM/DD");

        while (i < _self.scale.value) {
            var d = moment.duration(i, _self.scale.units);
            var c = _self.data.startDate.clone().add(d);
            var len = _self.scale.getLength(d);
            var isMajor = _self.scale.isMajor(c);
            var currentDay = c.format("MM/DD");
            
            if (isMajor) {
                ctx.fillStyle = "#aaaaaa";
                ctx.textAlign = "right";

                // check to ensure a tick mark label is not too close to the left side start date text - skip if closer than 70 pixels
                if (len > 70) {
                    if (prevDay != currentDay)
                        ctx.fillText(currentDay, scaleLeft + len - 2, scaleTop - 10);

                    prevDay = currentDay;

                    ctx.fillText(c.format("hh:mm a"), scaleLeft + len - 2, scaleTop);
                }

                // major tick mark color
                ctx.strokeStyle = "#aaaaaa";
            } else {
                // minor tick mark color
                ctx.strokeStyle = "#dfdfdf";
            }

            // draws the vertical tick mark line (major or minor)
            ctx.beginPath();
            ctx.moveTo(scaleLeft + len, (isMajor) ? verticalLineTop : horizontalLineTop);
            ctx.lineTo(scaleLeft + len, canvas.height);
            ctx.stroke();

            i += _self.scale.increment;
        }

        // get the length of the total chart duration - because we want a terminating line on the right
        var len = _self.scale.getTotalLength();

        // draw the vertical line at the end of the date range
        ctx.strokeStyle = "#000000";
        ctx.beginPath();
        ctx.moveTo(scaleLeft + len, verticalLineTop);
        ctx.lineTo(scaleLeft + len, canvas.height);
        ctx.stroke();

        // draw the end date text
        ctx.fillStyle = "#000000";
        ctx.textAlign = "left";
        ctx.fillText(_self.data.endDate.format("MM/DD"), scaleLeft + len + 5.5, scaleTop - 10);
        ctx.fillText(_self.data.endDate.format("hh:mm a"), scaleLeft + len + 5.5, scaleTop);
    };

    var drawDurations = function (canvas, ctx) {
        /// <summary>Draws event durations on the chart.</summary>
        /// <param name="canvas" type="HTMLCanvasElement">The canvas on which to draw.</param>
        /// <param name="ctx" type="CanvasRenderingContext2D">The current drawing context.</param>
        
        ctx.textAlign = "right";

        var line = scaleTop + 26;
        var len = _self.scale.getTotalLength();

        var drawDurationRectangle = function (info) {
            /// <summary>Draws a single reservation duration rectangle.</summary>
            /// <param name="info" type="DurationInfo">A reservation comprised of one or more duration parts.</param>
            
            var left = scaleLeft - 0.5;
            
            $.each(info.parts, function (index, item) {
                var clr = _self.colors[item.durationType];
                var width = _self.scale.getLength(item.duration); // the width of this DurationPart (item)
                var diff = info.start.diff(_self.data.startDate);
                var dur = moment.duration(diff); // the duration between 
                var x = left + _self.scale.getLength(dur);
                ctx.fillStyle = clr;
                ctx.fillRect(x, line - 12, width, 15);
                left += width;
            });
        }

        $.each(_self.data.durations, function (index, item) {
            // draw the left side ReservationID text
            ctx.fillStyle = "#000000";
            ctx.textAlign = "right";
            ctx.fillText(item.reservationId, scaleLeft - 5.5, line);
            
            drawDurationRectangle(item)

            // draw the right side ReservationID text
            ctx.fillStyle = "#000000";
            ctx.textAlign = "left";
            ctx.fillText(item.reservationId, scaleLeft + len + 5.5, line);

            // draw the horizontal line below the reservation
            ctx.beginPath();
            ctx.moveTo(0, line + 10.5);
            ctx.lineTo(canvas.width, line + 10.5);
            ctx.strokeStyle = "#aaaaaa";
            ctx.stroke();
            
            // move to the next line
            line += 30;
        });
    };

    _self.render = function (target) {
        /// <summary>Renders the chart on the specified target canvas.</summary>
        /// <param name="target" type="HTMLCanvasElement">The canvas on which to draw.</param>

        var context = target.getContext("2d");

        // need to constrain max width/height because if the canvas is too big nothing is displayed (this is a browser limitation)
        var maxWidth = 32767;
        var maxHeight = 10000;

        var len = _self.scale.getTotalLength();
        
        target.width = Math.min((2 * scaleLeft) + len, maxWidth);
        target.height = Math.min(scaleTop + 7 + (_self.data.durations.length * 30), maxHeight);

        if (_self.backgroundColor) {
            context.fillStyle = _self.backgroundColor;
            context.fillRect(0, 0, target.width, target.height);
        }

        drawScale(target, context);
        drawDurations(target, context);
    };
}

(function ($) {
    $.fn.durations = function (options) {
        /// <summary>Creates a DurationChart in each selected element.</summary>
        /// <param name="options" value="{startDate:'',endDate:'',durations:[{ReservationID:0,ChargeBeginDateTime:'',Parts:[{DurationType:'',Duration:''}]}],pixelsPerUnit:0,backgroundColor:'',colors:{C1:'',C2:''}}">The chart options.</param>

        return this.each(function () {
            var $this = $(this);
            
            var opts = $.extend({}, { "startDate": null, "endDate": null, "durations": null, "pixelsPerUnit": 4, "backgroundColor": null, "colors": null }, options, $this.data());
            
            if (opts.durations) {
                var chart = new DurationChart(opts);
                var canvas = document.createElement("canvas");
                chart.render(canvas);
                $this.html(canvas);
            }
        });
    };
}(jQuery));
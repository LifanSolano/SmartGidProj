ymaps.ready(init);
var myMap;

function update() {
    myMap = new ymaps.Map('map', {
        center: [49.806406, 73.085485],
        zoom: 12,
        controls: ['zoomControl', 'searchControl', 'typeSelector', 'fullscreenControl']
    }, {
        balloonMaxWidth: 200,

        searchControlProvider: 'yandex#search'
    }),
        objectManager = new ymaps.ObjectManager();

    myMap.geoObjects.add(objectManager);

    $.ajax({
        url: "/Content/data.json"
    }).done(function (data) {
        objectManager.add(data);
    });
}

function updateAddObject() {
    $.ajax({
        url: "/Content/data.json"
    }).done(function (data) {
        objectManager.add(data);
    });
}

function removeObject() {
    $.ajax({
        url: "/Content/data.json"
    }).done(function (data) {
        objectManager.remove(data);
    });
}

function init () {
    myMap = new ymaps.Map('map', {
        center: [49.806406, 73.085485],
        zoom: 12,
        controls: ['zoomControl', 'searchControl', 'typeSelector',  'fullscreenControl']
    }, {
        balloonMaxWidth: 200,
        // При сложных перестроениях можно выставить автоматическое
        // обновление карты при изменении размеров контейнера.
        // При простых изменениях размера контейнера рекомендуется обновлять карту программно.
        // autoFitToViewport: 'always'
        searchControlProvider: 'yandex#search'
    }),
        objectManager = new ymaps.ObjectManager();

    myMap.geoObjects.add(objectManager);

    $.ajax({
        url: "/Content/data.json"
    }).done(function(data) {
        objectManager.add(data);
    });

    function UpdateMap() {
        myMap.destroy();
        update();
    }

    $(document).on("click", ".update-map", function () {
        myMap.destroy();
        update();
    });

    $(document).on("click",".btnDelete", function () {
        var id = $(this).attr("iid");
        var tower = {
            "ID": id
        };
        $.ajax({
            type: "post",
            url: "Home/DeleteTower",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({ tower: tower }),
            error: function (xhr, textStatus, exceptionThrown) {
                var errorData = $.parseJSON(xhr.responseText);
                var errorMessages = [];
                //this ugly loop is because List<> is serialized to an object instead of an array
                for (var key in errorData) {
                    errorMessages.push(errorData[key]);
                }
                //$('#towerTitle').html(errorMessages.join("<br />"));
            }
        });
        myMap.destroy();
        update();
    });

    $(document).on("click", ".btnMove", function () {
        //сначала храним старые координаты
        var tower = {
            "ID": $(this).attr("iid"),
            "Lat": $("[name='coord1']").val(),
            "Long": $("[name='coord2']").val()
        };

        myMap.events.add('click', function (e) {
            debugger;
            var coords = e.get('coords');
            $("[name='coord1']").val(coords[0]);
            $("[name='coord2']").val(coords[1]);
            var sameCoords = false; //флажок совпадения координат
            var lat = 0, //широта
                long = 0; //долгота
            $.getJSON("/Content/data.json", function (data) {
                for (var i=0; i<data.features.length; i++)
                {
                    lat = data.features[i].geometry.coordinates[0];
                    long = data.features[i].geometry.coordinates[1];
                    if (lat == tower.Lat && long == tower.Long) //на всякий случай проверка на схожесть местоположений
                    {
                        sameCoords = true;
                        break;
                    }
                    else
                    {
                        sameCoords = false;
                    }
                }
                if (sameCoords == true)
                {
                    alert("Извините, нельзя переместить объект на одно и то же место");
                    return;
                }
                else
                {
                    tower.Lat = parseFloat($("[name='coord1']").val());
                    tower.Long = parseFloat($("[name='coord2']").val());
                    $.ajax({
                        type: "post",
                        url: "Home/MoveTower",
                        dataType: "json",
                        contentType: "application/json",
                        data: JSON.stringify({ tower: tower }),
                        error: function (xhr, textStatus, exceptionThrown) {
                            var errorData = $.parseJSON(xhr.responseText);
                            var errorMessages = [];
                            //this ugly loop is because List<> is serialized to an object instead of an array
                            for (var key in errorData) {
                                errorMessages.push(errorData[key]);
                            }
                            //$('#towerTitle').html(errorMessages.join("<br />"));
                        }
                    });
                }
            });
        });
        
        //myMap.events.add('click', function (e) {
        //    var coords = e.get('coords');
        //    $("[name='coord1']").val(coords[0]);
        //    $("[name='coord2']").val(coords[1]);
        //    $.getJSON("data.json", function (data) {
        //        /*data.push(data.features[id].geometry.coordinates[0] = coords[0]);*/
        //        let balloon = data.features[id].geometry;
        //        debugger;
        //        if (balloon.coordinates[0] == tower.Lat && balloon.coordinates[1] == tower.Long) {
        //            debugger;
        //        }
        //    });
        //});

        
    });

    var removeElements = function (text, selector) {
        var wrapped = $("<div>" + text + "</div>");
        wrapped.find(selector).remove();
        return wrapped.html();
    }

    
    //$("#inputFile").on("change", function () { //вытаскиваем название файла
    //    var filePath = this.files[0].name;
    //});
    
    //добавление нового ЛЭПа
    $("#addTowerBtn").on("click", function () {
        var tower = {
            "Name": $("#towerTitle").val(),
            "Lat": parseFloat($("#latitude").val()),
            "Long": parseFloat($("#longitude").val()),
            "FilePath": $('#inputFile').val().replace(/C:\\fakepath\\/, '')
        };
        //$.getJSON("data.json", function (data) {
        //    $.each(function () {
        //        data(data.features.properties);
        //    })
        //});

        //добавление собранной даты
        $.ajax({
            type: "post",
            url: "Home/AddNewTower",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({ tower: tower }),
            success: function(success){
                updateAddObject();
            },
            error: function (xhr, textStatus, exceptionThrown) {
                var errorData = $.parseJSON(xhr.responseText);
                var errorMessages = [];
                //this ugly loop is because List<> is serialized to an object instead of an array
                for (var key in errorData) {
                    errorMessages.push(errorData[key]);
                }
                $('#towerTitle').html(errorMessages.join("<br />"));
            }
        });
        $.fancybox.close();
    });
    
    //вытаскиваем все ЛЭПы для удаления в выпадающий список
    $("#removeTowerWindow").on("click", function () {
        $("#selectNameTowerRemove").empty();
        //добавление собранной даты
        $.ajax({
            type: "get",
            url: "Home/GetTowersList",
            dataType: "json",
            success: function (towersList) {
                $.each(towersList, function (i, value) {
                    $("#selectNameTowerRemove").append($('<option id=' + i + '>').text(value["Name"]).attr('value', value["Name"]));
                });
            },
            error: function (xhr, textStatus, exceptionThrown) {
                var errorData = $.parseJSON(xhr.responseText);
                var errorMessages = [];
                //this ugly loop is because List<> is serialized to an object instead of an array
                for (var key in errorData) {
                    errorMessages.push(errorData[key]);
                }
                //$('#towerTitle').html(errorMessages.join("<br />"));
            }
        });
        $.fancybox.close();
    });

    //удаление ЛЭПа
    $("#removeTowerBtn").on("click", function () {
        var tower = {
            "Name": $("#selectNameTowerRemove :selected").text()
        };
        
        //добавление собранной даты
        $.ajax({
            type: "post",
            url: "Home/DeleteTower",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({ tower: tower }),
            success: function (success) {
                UpdateMap();
            },
            error: function (xhr, textStatus, exceptionThrown) {
                var errorData = $.parseJSON(xhr.responseText);
                var errorMessages = [];
                //this ugly loop is because List<> is serialized to an object instead of an array
                for (var key in errorData) {
                    errorMessages.push(errorData[key]);
                }
                $('#towerTitle').html(errorMessages.join("<br />"));
            }
        });
        $.fancybox.close();
    });

    //вывод списка ЛЭПов в выпадающий список для перемещения
    $("#moveTowerWindow").on("click", function () {
        $("#selectNameTowerMove").empty();
        //добавление собранной даты
        $.ajax({
            type: "get",
            url: "Home/GetTowersList",
            dataType: "json",
            success: function (towersList) {
                $.each(towersList, function (i, value) {
                    $("#selectNameTowerMove").append($('<option id=' + i + '>').text(value["Name"]).attr('value', value["Name"]));
                });
            },
            error: function (xhr, textStatus, exceptionThrown) {
                var errorData = $.parseJSON(xhr.responseText);
                var errorMessages = [];
                //this ugly loop is because List<> is serialized to an object instead of an array
                for (var key in errorData) {
                    errorMessages.push(errorData[key]);
                }
                //$('#towerTitle').html(errorMessages.join("<br />"));
            }
        });
        $.fancybox.close();
    });

    //перемещение ЛЭПа
    $("#moveTowerBtn").on("click", function () {
        var tower = {
            "Name": $("#selectNameTowerMove :selected").text(),
            "Lat": parseFloat($("#latitudeMove").val()),
            "Long": parseFloat($("#longitudeMove").val())
        };
        debugger;
        //добавление собранной даты
        $.ajax({
            type: "post",
            url: "Home/MoveTower",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({ tower: tower }),
            success: function (success) {
                UpdateMap();
            },
            error: function (xhr, textStatus, exceptionThrown) {
                var errorData = $.parseJSON(xhr.responseText);
                var errorMessages = [];
                //this ugly loop is because List<> is serialized to an object instead of an array
                for (var key in errorData) {
                    errorMessages.push(errorData[key]);
                }
                $('#towerTitle').html(errorMessages.join("<br />"));
            }
        });
        $.fancybox.close();
    });
    
    //вывод списка ЛЭПов в левый блок
    $.ajax({
        type: "get",
        url: "Home/GetTowersList",
        success: function (towers) {
            var items = '';
            $.each(towers, function (key, val) {
                items += '<li id="' + key + '"><a href="#">' + val.Name + '</a></li>';
            });
            $("#towersListUl").append(items);
        },
        error: function (error) {
            console.log('Error:    ', error);
        }
    });

    myMap.events.add('click', function (e) {
        var coords = e.get('coords');
        $("[name='latitude']").val(coords[0]);
        $("[name='longitude']").val(coords[1]);
    });

    /*Координаты по клику*/

    /*myMap.events.add('click', function (e) {
        if (!myMap.balloon.isOpen()) {
            var coords = e.get('coords');
            $("[name='latitude']").val(coords[0]);
            $("[name='longitude']").val(coords[1]);
            MyIconContentLayout = ymaps.templateLayoutFactory.createClass(
                '<div style="color: #FFFFFF; font-weight: bold;">$[properties.iconContent]</div>'
            ),
                myPlacemark = new ymaps.Placemark([coords[0], coords[1]], {
                    hintContent: 'Собственный значок метки',
                    balloonContent: 'Это красивая метка'
                }, {
                    // Опции.
                    // Необходимо указать данный тип макета.
                    iconLayout: 'default#image',
                    // Своё изображение иконки метки.
                    iconImageHref: 'images/electric-tower.png',
                    // Размеры метки.
                    iconImageSize: [30, 42],
                    // Смещение левого верхнего угла иконки относительно
                    // её "ножки" (точки привязки).
                    iconImageOffset: [-5, -38]
                });
            myMap.geoObjects.add(myPlacemark);

            myMap.balloon.open(coords,  {
                contentHeader: 'Событие!',
                contentBody: '<p>Кто-то щелкнул по карте.</p>' +
                '<p>Координаты щелчка: ' + [
                    coords[0].toPrecision(6),
                    coords[1].toPrecision(6)
                ].join(', ') + '</p>',
                contentFooter: '<sup>Для Щелкните еще раз</sup>'
            });
        }
        else {
            myMap.balloon.close();
        }
    });*/
    // Обработка события, возникающего при щелчке
    // правой кнопки мыши в любой точке карты.
    // При возникновении такого события покажем всплывающую подсказку
    // в точке щелчка.
    myMap.events.add('contextmenu', function (e) {
        myMap.hint.open(e.get('coords'), 'Кто-то щелкнул правой кнопкой');
    });

    // Скрываем хинт при открытии балуна.
    myMap.events.add('balloonopen', function (e) {
        myMap.hint.close();
    });
}
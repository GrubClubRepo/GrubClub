
var testApp = angular.module('testApp',[]).
controller('MyCtrl', ['$scope', '$http', 
    function($scope, $http) {
    $scope.testdata = ['lorem', 'ipsum'];
        // var resturl = 'http://grubclub.com/api/searchallevents?city=london&pageIndex=1&resultsPerPage=15';
         var localurl = 'http://localhost:1797/api/searchallevents?city=london&pageIndex=1&resultsPerPage=2';
    var encodedurl = 'http%3A%2F%2Fgrubclub.com%2Fapi%2Fsearchallevents%3Fcity%3Dlondon%26pageIndex%3D1%26resultsPerPage%3D15';
    var localencodedurl = 'http%3A%2F%2Flocalhost%3A1797%2Fapi%2Fsearchallevents%3Fcity%3Dlondon%26pageIndex%3D1%26resultsPerPage%3D15';
    //var YQLurl = "http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20json%20where%20url%3D'localhost%3A1797%2Fapi%2Fsearchallevents%3Fcity%3Dlondon%26pageIndex%3D1%26resultsPerPage%3D15'&format=json&callback=JSON_CALLBACK";
    $http.get(localurl)
        .success(function(data) {
            alert("success");
            console.log(JSON.stringify(data));
           // $scope.events = data;
        });    
}]);
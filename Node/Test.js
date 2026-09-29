const {add} = require("./Math.js"); //Math.js에서 add함수를 가져옴

var num = 42;
var name = "TOM";
let isStudent = true;

//배열
let color = ["red", "green", "blue"];
//객체
let person = {name : "Alice", age : 30};

console.log(add(num,num)); //함수호출

//함수
function greet(name)
{
    console.log("Hello, " + name + "!");
}
//함수호출
greet(person.name);

//조건문
if(num>30)
{
    console.log(" Number is greater than 30");
}
else
{
    console.log(" Number is lower than 30");
}

//반복분
for(var i = 0; i < 5 ; i++)
{
    console.log(i);
}

//비동기 콜백

setTimeout(()=> {
    console.log("Delayed Message 1");
},1000); //1초

setTimeout(()=> {
    console.log("Delayed Message 2");
},750); //0.7초

setTimeout(()=> {
    console.log("Delayed Message 1");
},2000); //2초

setTimeout(()=> {
    console.log("Delayed Message 1");
},500); //0.5초
let express = require('express');
let app = express();

app.get('/', function(req, res) {
    res.send('Hello, World!');
});

app.get('/about', function(req, res) {
    res.send('Player Data 0010101010');
});

app.listen(3000, function() {
    console.log('Server is running on http://localhost:3000');
}); 
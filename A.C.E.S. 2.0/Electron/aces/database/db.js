var mysql = require('mysql');

// Add the credentials to access your database
var connection = mysql.createConnection({
    host     : 'localhost',
    user     : 'admin',
    password : 'ACES2.0password', // or the original password : 'apaswword'
    database : 'ACES'
});

connection.connect();

$sql = 'SELECT `username`, `password`, `creationDate` FROM `users`';

connection.query($sql, function (error, results, fields) {
  if (error) throw error;
  console.log(results);
  $('#resultDiv').text(results[0].username);
});
 
connection.end();
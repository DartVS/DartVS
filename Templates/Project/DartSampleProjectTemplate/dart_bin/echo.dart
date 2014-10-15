import 'dart:io';

main() {
  print('');
  print('');
  print('Welcome to echo!');
  print('');
  print('Type a message and press <ENTER>.');
  print('');

  while (true) {
    stdout.write('> ');
    print('  ' + stdin.readLineSync());
  }
}

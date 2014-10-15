import 'package:intl/intl.dart';

var dayFormatter = new DateFormat.EEEE();

main() {
  var day = dayFormatter.format(new DateTime.now());
  print('');
  print('Today is $day');
  print('');
}

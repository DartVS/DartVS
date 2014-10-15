import 'dart:async';
import 'package:unittest/unittest.dart';

void main() {
  test('Passing top-level #1', () {
    expect(1, equals(1));
  });

  group('Sync tests', () {
    test('Passing sync test', () {
      expect(1, equals(1));
    });

    test('Failing sync test #1', () {
      expect(1, equals(2));
    });
  });

  group('Async tests', () {
    test('Passing async test', () {
      new Future.delayed(new Duration(seconds: 1), expectAsync(() {
        expect(1, equals(1));
      }));
    });

    test('Passing slow async test', () {
       new Future.delayed(new Duration(seconds: 5), expectAsync(() {
        expect(1, equals(1));
      }));
    });

    test('Failing async test', () {
      new Future.delayed(new Duration(seconds: 1), expectAsync(() {
        expect(1, equals(2));
      }));
    });
  });
}

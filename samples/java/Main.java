/**
 * サンプル (Java)
 *  Commandには、java Main.javaのフルパス
 *  または、nodejsのバッチを参考にバッチを作成する
 */

import java.util.*;

class Main {
  public static void main(String... args) {
    var stdin = new Scanner(System.in);
    var meta = stdin.nextLine().split(" ", 0);
    int Y = Integer.parseInt(meta[1]);
    char[][] state = new char[Y][];
    for (int y = 0; y < Y; ++y) {
      state[y] = stdin.nextLine().toCharArray();
    }
    stdin.close();

    // debug output
    System.err.println(meta[2]);
    System.err.println(Arrays.deepToString(state));
    aiproc(state, meta[2]);
  }

  /**
   * main procedure
   * @param state status
   * @param A 
   */
  private static void aiproc(char[][] state, String A) {
    System.out.println('0');
  }
}
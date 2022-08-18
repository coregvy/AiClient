/**
 * サンプル（nodejs）
 *   Commandには、sample.batのフルパス、または
 *   "node C:\hoge\index.js"
 *   を入力する
 */

'use strict';

process.stdin.resume();
process.stdin.setEncoding('utf8');
let X = -1, Y, A;
const state = [];
process.stdin.on('data', function(chunk) {
  chunk.split(/\n/).forEach(line => {
    // 改行コードが\r\nの場合、\rを除く
    line = line.replace(/\r/, '');
    if (line.length == 0) return;
    if (X < 0) {
      // 1行目の入力
      [X, Y, A] = line.split(' ');
    } else {
      state.push(line.split(''));
      // 最後の行に到達したか
      if (state.length == Y) {
        // debug output
        console.error('x, y, a: ', X, Y, A);
        console.error('state: \n' + state.map(r=>r.join('-')).join('\n'));
        aiproc(state, A);
        process.exit();
      }
    }  
  });
});

/**
 * メイン処理
 * @param {string[][]} state 盤面の状態
 * @param {string} A 自分の記号
 */
function aiproc(state, A) {
  console.log(Math.floor(Math.random() * state[0].length));
}

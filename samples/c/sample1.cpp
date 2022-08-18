/*******************************
 * サンプル（C言語）
 *   Commandには、ビルドしたsample1.exeの
 *   絶対パスを入力する
********************************/

#include <stdio.h>
#include <stdlib.h>	// rand
#include <time.h>	// time

int main() {
	// get input
	int X, Y, A;
	scanf("%d %d %d", &X, &Y, &A);
	char **S = new char*[Y + 1];
	
	for (int y = 0; y < Y; ++y) {
		S[y] = new char[X + 1];
		scanf("%s", S[y]);
	}
	
	// todo
	srand((unsigned int)time(NULL));
	printf("%d\n", rand() % Y);
	return 0;
}

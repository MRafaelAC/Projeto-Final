Este projeto consiste num jogo 2D feito em monogame pelos Alunos:
- Mário Rafael Azevedo Costa nº31467
- João Costa nº31489

O jogo consite num remake simples do clássico Space Invaders, feito em MonoGame. Controla a tua nave, destrói os inimigos e tenta bater o recorde!

Funcionalidades

-Controlo lateral da nave com as teclas ← →.

-Disparo com Barra de Espaço.

-Inimigos que se movem lateralmente.

-Progreção de dificuldade com os inimigos a dispararem mais rapidamente.

-Sistema de pontuação e High Score guardado em ficheiro.

-Reinício automático ao ser atingido.


Estrutura

Game1.cs	- Lógica principal do jogo (input, atualização, rendering, lógica geral)

Enemy.cs	- Classe dos inimigos

Bullet.cs	- Classe das balas (jogador e inimigos)

Content/	- Imagens (player.png, enemy.png, bullet.png) e fonte (font.spritefont)

highscore.txt	- Guarda o recorde (criado automaticamente na primeira execução)


Teclas Para Jogar

← / → — Mover nave.

Espaço — Disparar.

Esc — Sair do jogo.

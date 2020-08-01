using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tabuleiro;

namespace xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro tab { get; private set; }
        public int turno { get; private set; }
        public Cor jogadorAtual { get; private set; }
        public bool terminada { get; private set; }
        private HashSet<Peca> pecas;
        private HashSet<Peca> capturadas;
        public bool xeque { get; private set; }

        public PartidaDeXadrez()
        {
            tab = new Tabuleiro(8, 8);
            turno = 1;
            jogadorAtual = Cor.Amarelo;
            terminada = false;
            xeque = false;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            colocarPecas();
            
        }

        public Peca executaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.retirarPeca(origem);
            p.incrementarQntMovimentos();
            Peca pecaCapturada = tab.retirarPeca(destino);
            tab.colocarPeca(p, destino);
            if(pecaCapturada != null)
            {
                capturadas.Add(pecaCapturada);
            }
            //jogada especial roque pequeno
            if(p is Rei && destino.coluna == origem.coluna + 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna + 3);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna + 1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQntMovimentos();
                tab.colocarPeca(T, destinoT);
            }
            //jogada especial roque grande
            if (p is Rei && destino.coluna == origem.coluna - 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna - 4);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna -1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQntMovimentos();
                tab.colocarPeca(T, destinoT);
            }
            return pecaCapturada;
        }

        public void desfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = tab.retirarPeca(destino);
            p.decrementarQntMovimentos();
            if(pecaCapturada != null)
            {
                tab.colocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }
            tab.colocarPeca(p, origem);
            //jogada especial roque pequeno
            if (p is Rei && destino.coluna == origem.coluna + 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna + 3);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna + 1);
                Peca T = tab.retirarPeca(destinoT);
                T.decrementarQntMovimentos();
                tab.colocarPeca(T, origemT);
            }
            //jogada especial roque grande
            if (p is Rei && destino.coluna == origem.coluna - 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna - 4);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna - 1);
                Peca T = tab.retirarPeca(destinoT);
                T.incrementarQntMovimentos();
                tab.colocarPeca(T, origemT);
            }
        }

        public void realizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = executaMovimento(origem, destino);

            if (estaEmXeque(jogadorAtual))
            {
                desfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Voce nao pode se colocar em xeque.");
            }
            if (estaEmXeque(adversaria(jogadorAtual)))
            {
                xeque = true;
            }
            else
            {
                xeque = false;
            }
            if (testeXequemate(adversaria(jogadorAtual)))
            {
                terminada = true;
            }
            else
            {
                turno++;
                mudaJogador();
            }
          
        }
        public void validarPosicaoDeOrigem(Posicao pos)
        {
            if(tab.peca(pos) == null)
            {
                throw new TabuleiroException("Não existe peça nessa posição.");
            }if(jogadorAtual != tab.peca(pos).cor)
            {
                throw new TabuleiroException("A peça escolhida não é sua!");
            }
            if (!tab.peca(pos).existeMovimentosPossiveis())
            {
                throw new TabuleiroException("Não existe movimentos possíveis para esta peça.");
            }
        }
        public void validarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if (!tab.peca(origem).movimentoPossivel(destino))
            {
                throw new TabuleiroException("Posição de destino não é valida.");
            }
        }

        private void mudaJogador()
        {
            if(jogadorAtual == Cor.Amarelo)
            {
                jogadorAtual = Cor.Verde;
            }
            else
            {
                jogadorAtual = Cor.Amarelo;
            }
        }
        public HashSet<Peca> pecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach(Peca x in capturadas)
            {
                if(x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            return aux;
        }
        public HashSet<Peca> pecasEmJogo(Cor cor)//rever esta parte depois
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in pecas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(pecasCapturadas(cor));
            return aux;
        }
        private Cor adversaria(Cor cor)
        {
            if(cor == Cor.Amarelo)
            {
                return Cor.Verde;
            }
            else
            {
                return Cor.Amarelo;
            }
        }
        private Peca rei(Cor cor)
        {
            foreach(Peca x in pecasEmJogo(cor))
            {
                if(x is Rei)
                {
                    return x;
                }
            }
            return null;
        }

        public bool estaEmXeque(Cor cor)
        {
            Peca R = rei(cor);
            if(R == null)
            {
                throw new TabuleiroException("Não tem rei da cor " + cor + " no tabuleiro.");
            }
            foreach(Peca x in pecasEmJogo(adversaria(cor)))
            {
                bool[,] mat = x.movimentosPossiveis();
                if(mat[R.posicao.linha, R.posicao.coluna])
                {
                    return true;
                }
            }
            return false;
        }
        public bool testeXequemate(Cor cor)
        {
            if (!estaEmXeque(cor))
            {
                return false;
            }
            foreach(Peca x in pecasEmJogo(cor))
            {
                bool[,] mat = x.movimentosPossiveis();
                for(int i = 0; i<tab.linhas; i++)
                {
                    for(int j= 0; j< tab.colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = x.posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = executaMovimento(origem, destino );
                            bool testeXeque = estaEmXeque(cor);
                            desfazMovimento(origem, destino, pecaCapturada);
                            if (!testeXeque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        public void colocarNovaPeca(char coluna, int linha, Peca peca) 
        {
            tab.colocarPeca(peca, new PosicaoXadrez(coluna, linha).toPosicao());
            pecas.Add(peca);
        }
        private void colocarPecas()
        {
            colocarNovaPeca('a', 1, new Torre(tab, Cor.Amarelo));
            colocarNovaPeca('b', 1, new Cavalo(tab, Cor.Amarelo));
            colocarNovaPeca('c', 1, new Bispo(tab, Cor.Amarelo));
            colocarNovaPeca('d', 1, new Dama(tab, Cor.Amarelo));
            colocarNovaPeca('e', 1, new Rei(tab, Cor.Amarelo, this));
            colocarNovaPeca('f', 1, new Bispo(tab, Cor.Amarelo));
            colocarNovaPeca('g', 1, new Cavalo(tab, Cor.Amarelo));
            colocarNovaPeca('h', 1, new Torre(tab, Cor.Amarelo));
            colocarNovaPeca('a', 2, new Peao(tab, Cor.Amarelo));
            colocarNovaPeca('b', 2, new Peao(tab, Cor.Amarelo));
            colocarNovaPeca('c', 2, new Peao(tab, Cor.Amarelo));
            colocarNovaPeca('d', 2, new Peao(tab, Cor.Amarelo));
            colocarNovaPeca('e', 2, new Peao(tab, Cor.Amarelo));
            colocarNovaPeca('f', 2, new Peao(tab, Cor.Amarelo));
            colocarNovaPeca('g', 2, new Peao(tab, Cor.Amarelo));
            colocarNovaPeca('h', 2, new Peao(tab, Cor.Amarelo));

            colocarNovaPeca('a', 8, new Torre(tab, Cor.Verde));
            colocarNovaPeca('b', 8, new Cavalo(tab, Cor.Verde));
            colocarNovaPeca('c', 8, new Bispo(tab, Cor.Verde));
            colocarNovaPeca('d', 8, new Dama(tab, Cor.Verde));
            colocarNovaPeca('e', 8, new Rei(tab, Cor.Verde, this));
            colocarNovaPeca('f', 8, new Bispo(tab, Cor.Verde));
            colocarNovaPeca('g', 8, new Cavalo(tab, Cor.Verde));
            colocarNovaPeca('h', 8, new Torre(tab, Cor.Verde));
            colocarNovaPeca('a', 7, new Peao(tab, Cor.Verde));
            colocarNovaPeca('b', 7, new Peao(tab, Cor.Verde));
            colocarNovaPeca('c', 7, new Peao(tab, Cor.Verde));
            colocarNovaPeca('d', 7, new Peao(tab, Cor.Verde));
            colocarNovaPeca('e', 7, new Peao(tab, Cor.Verde));
            colocarNovaPeca('f', 7, new Peao(tab, Cor.Verde));
            colocarNovaPeca('g', 7, new Peao(tab, Cor.Verde));
            colocarNovaPeca('h', 7, new Peao(tab, Cor.Verde));


        }
    }
}

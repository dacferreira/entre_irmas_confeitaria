import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CabecalhoComponent } from '@components/cabecalho/cabecalho.component';
import { RodapeComponent } from '@components/rodape/rodape.component';
import { SacolaComponent } from '@components/sacola/sacola.component';

@Component({
  selector: 'eirm-root',
  imports: [RouterOutlet, CabecalhoComponent, RodapeComponent, SacolaComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {}

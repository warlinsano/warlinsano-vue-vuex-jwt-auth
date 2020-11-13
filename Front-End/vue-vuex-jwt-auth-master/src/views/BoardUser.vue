<template>
  <div class="container">
    <header class="jumbotron">
      <h3>Lista de Usuarios</h3>
    </header>
    <p>
      <a class="btn btn-primary">Buscar </a>
      <input type="text" class="from-control" placeholder="Escriba para buscar">
    </p>
    <table class="table">
      <thead>
        <tr>
          <th scope="col">#</th>
          <th scope="col">User Name</th>
          <th scope="col">Email</th>
          <th scope="col">Email Confirmed</th>
          <th scope="col">photo</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(user,index) in content" :key="index">
          <th scope="row">1</th>
          <td>{{user.username}}</td>
          <td>{{user.email}}</td>
          <td>{{user.emailConfirmed}}</td>
          <td><img width="30" height="30"  src="//ssl.gstatic.com/accounts/ui/avatar_2x.png"
                    class="profile"></td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
import UserService from '../services/user.service';

export default {
  name: 'User',
  data() {
    return {
      content: ''
    };
  },
  mounted() {
    UserService.getUserBoard().then(
      response => {
        this.content = response.data;
      },
      error => {
        this.content =
          (error.response && error.response.data && error.response.data.message) ||
          error.message ||
          error.toString();
      }
    );
  }
};
</script>

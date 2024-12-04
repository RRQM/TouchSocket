
import React from 'react';
import '../css/Paypal.css';

const Paypal = () => {

    return (

        <div>
            <a href="https://www.paypal.com/ncp/payment/R2DP7NLY3KPG2" target="_blank">
                <button class="button">付款</button>
            </a>

            <div class="image-container">
                <img src={require('@site/static/img/docs/enterprise-2.png').default} width="200" />
            </div>
        </div>
    );
};

export default Paypal;